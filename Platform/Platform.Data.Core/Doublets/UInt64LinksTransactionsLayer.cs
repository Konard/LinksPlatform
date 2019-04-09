﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Platform.Helpers;
using Platform.Helpers.Disposables;
using Platform.Helpers.IO;
using System.Linq;

namespace Platform.Data.Core.Doublets
{
    public class UInt64LinksMemoryMenagerTransactionsLayer : DisposableBase, ILinksMemoryManager<ulong>
    {
        /// <remarks>
        /// Альтернативные варианты хранения трансформации (элемента транзакции):
        /// 
        /// private enum TransitionType
        /// {
        ///     Creation,
        ///     UpdateOf,
        ///     UpdateTo,
        ///     Deletion
        /// }
        /// 
        /// private struct Transition
        /// {
        ///     public ulong TransactionId;
        ///     public UniqueTimestamp Timestamp;
        ///     public TransactionItemType Type;
        ///     public Link Source;
        ///     public Link Linker;
        ///     public Link Target;
        /// }
        /// 
        /// Или
        /// 
        /// public struct TransitionHeader
        /// {
        ///     public ulong TransactionIdCombined;
        ///     public ulong TimestampCombined;
        /// 
        ///     public ulong TransactionId
        ///     {
        ///         get
        ///         {
        ///             return (ulong) mask & TransactionIdCombined;
        ///         }
        ///     }
        /// 
        ///     public UniqueTimestamp Timestamp
        ///     {
        ///         get
        ///         {
        ///             return (UniqueTimestamp)mask & TransactionIdCombined;
        ///         }
        ///     }
        /// 
        ///     public TransactionItemType Type
        ///     {
        ///         get
        ///         {
        ///             // Использовать по одному биту из TransactionId и Timestamp,
        ///             // для значения в 2 бита, которое представляет тип операции
        ///             throw new NotImplementedException();
        ///         }
        ///     }
        /// }
        /// 
        /// private struct Transition
        /// {
        ///     public TransitionHeader Header;
        ///     public Link Source;
        ///     public Link Linker;
        ///     public Link Target;
        /// }
        /// 
        /// </remarks>
        public struct Transition
        {
            public static readonly long Size = UnsafeHelpers.SizeOf<Transition>();

            public ulong TransactionId;
            public UInt64Link Before;
            public UInt64Link After;
            public UniqueTimestamp Timestamp;

            public Transition(UniqueTimestampFactory uniqueTimestampFactory, ulong transactionId, UInt64Link before = default, UInt64Link after = default)
            {
                TransactionId = transactionId;
                Before = before;
                After = after;
                Timestamp = uniqueTimestampFactory.Create();
            }

            public override string ToString()
            {
                return $"{Timestamp} {TransactionId}: {Before} => {After}";
            }
        }

        /// <remarks>
        /// Другие варианты реализации транзакций (атомарности):
        ///     1. Разделение хранения значения связи ((Source Target) или (Source Linker Target)) и индексов.
        ///     2. Хранение трансформаций/операций в отдельной Links базе данных, но дополнительно потребуется решить вопрос
        ///        со ссылками на внешние идентификаторы, или как-то иначе решить вопрос с пересечениями идентификаторов.
        /// 
        /// Где хранить промежуточный список транзакций?
        /// 
        /// В оперативной памяти:
        ///  Минусы:
        ///     1. Может усложнить систему, если она будет функционировать самостоятельно,
        ///     так как нужно отдельно выделять память под список трансформаций.
        ///     2. Выделенной оперативной памяти может не хватить, в том случае,
        ///     если транзакция использует слишком много трансформаций.
        ///         -> Можно использовать жёсткий диск для слишком длинных транзакций.
        ///         -> Максимальный размер списка трансформаций можно ограничить / задать константой.
        ///     3. При подтверждении транзакции (Commit) все трансформации записываются разом создавая задержку.
        /// 
        /// На жёстком диске:
        ///  Минусы:
        ///     1. Длительный отклик, на запись каждой трансформации.
        ///     2. Лог транзакций дополнительно наполняется отменёнными транзакциями.
        ///         -> Это может решаться упаковкой/исключением дублирующих операций.
        ///         -> Также это может решаться тем, что короткие транзакции вообще
        ///            не будут записываться в случае отката.
        ///     3. Перед тем как выполнять отмену операций транзакции нужно дождаться пока все операции (трансформации)
        ///        будут записаны в лог.
        /// 
        /// </remarks>
        public class Transaction : DisposableBase
        {
            private readonly Queue<Transition> _transitions;
            private readonly UInt64LinksMemoryMenagerTransactionsLayer _layer;
            public bool IsCommitted { get; private set; }
            public bool IsReverted { get; private set; }

            public Transaction(UInt64LinksMemoryMenagerTransactionsLayer layer)
            {
                _layer = layer;

                if (_layer._currentTransactionId != 0)
                    throw new NotSupportedException("Nested transactions not supported.");

                IsCommitted = false;
                IsReverted = false;

                _transitions = new Queue<Transition>();

                SetCurrentTransaction(layer, this);
            }

            public void Commit()
            {
                EnsureTransactionAllowsWriteOperations(this);

                while (_transitions.Count > 0)
                {
                    var transition = _transitions.Dequeue();
                    _layer._transitions.Enqueue(transition);
                }

                _layer._lastCommitedTransactionId = _layer._currentTransactionId;

                IsCommitted = true;
            }

            private void Revert()
            {
                EnsureTransactionAllowsWriteOperations(this);

                var transitionsToRevert = new Transition[_transitions.Count];
                _transitions.CopyTo(transitionsToRevert, 0);

                for (var i = transitionsToRevert.Length - 1; i >= 0; i--)
                    _layer.RevertTransition(transitionsToRevert[i]);

                IsReverted = true;
            }

            public static void SetCurrentTransaction(UInt64LinksMemoryMenagerTransactionsLayer layer, Transaction transaction)
            {
                layer._currentTransactionId = layer._lastCommitedTransactionId + 1;
                layer._currentTransactionTransitions = transaction._transitions;
                layer._currentTransaction = transaction;
            }

            public static void EnsureTransactionAllowsWriteOperations(Transaction transaction)
            {
                if (transaction.IsReverted) throw new InvalidOperationException("Transation is reverted.");
                if (transaction.IsCommitted) throw new InvalidOperationException("Transation is commited.");
            }

            protected override void DisposeCore(bool manual)
            {
                if (manual)
                {
                    if (_layer != null && !_layer.IsDisposed)
                    {
                        if (!IsCommitted && !IsReverted)
                            Revert();

                        _layer.ResetCurrentTransation();
                    }
                }
            }
        }

        private static readonly TimeSpan DefaultPushDelay = TimeSpan.FromSeconds(0.1);

        private readonly string _logAddress;
        private readonly FileStream _log;
        private readonly Queue<Transition> _transitions;
        private readonly UniqueTimestampFactory _uniqueTimestampFactory;
        private readonly ILinksMemoryManager<ulong> _linksMemoryManager;
        private Task _transitionsPusher;
        private Transition _lastCommitedTransition;
        private ulong _currentTransactionId;
        private Queue<Transition> _currentTransactionTransitions;
        private Transaction _currentTransaction;
        private ulong _lastCommitedTransactionId;

        public ILinksCombinedConstants<bool, ulong, int> Constants => _linksMemoryManager.Constants;

        public UInt64LinksMemoryMenagerTransactionsLayer(ILinksMemoryManager<ulong> linksMemoryManager, string logAddress)
        {
            if (string.IsNullOrWhiteSpace(logAddress))
                throw new ArgumentNullException(nameof(logAddress));

            _linksMemoryManager = linksMemoryManager;

            // В первой строке файла хранится последняя закоммиченную транзакцию.
            // При запуске это используется для проверки удачного закрытия файла лога.
            // In the first line of the file the last committed transaction is stored.
            // On startup, this is used to check that the log file is successfully closed.
            var lastCommitedTransition = FileHelpers.ReadFirstOrDefault<Transition>(logAddress);

            var lastWrittenTransition = FileHelpers.ReadLastOrDefault<Transition>(logAddress);

            if (!Equals(lastCommitedTransition, lastWrittenTransition))
            {
                Dispose();
                throw new NotSupportedException("Database is damaged, autorecovery is not supported yet.");
            }

            if (Equals(lastCommitedTransition, default(Transition)))
                FileHelpers.WriteFirst(logAddress, lastCommitedTransition);

            _lastCommitedTransition = lastCommitedTransition;

            // TODO: Think about a better way to calculate or store this value
            var allTransitions = FileHelpers.ReadAll<Transition>(logAddress);
            _lastCommitedTransactionId = allTransitions.Max(x => x.TransactionId);

            _uniqueTimestampFactory = new UniqueTimestampFactory();

            _logAddress = logAddress;
            _log = FileHelpers.Append(logAddress);
            _transitions = new Queue<Transition>();
            _transitionsPusher = new Task(TransitionsPusher);
            _transitionsPusher.Start();
        }

        public ulong Count(IList<ulong> restrictions) => _linksMemoryManager.Count(restrictions);

        public bool Each(Func<ulong, bool> handler, IList<ulong> restrictions) => _linksMemoryManager.Each(handler, restrictions);

        public IList<ulong> GetLinkValue(ulong link) => _linksMemoryManager.GetLinkValue(link);

        public ulong AllocateLink()
        {
            var createdLinkIndex = _linksMemoryManager.AllocateLink();
            var createdLink = new UInt64Link(_linksMemoryManager.GetLinkValue(createdLinkIndex));
            CommitTransition(new Transition(_uniqueTimestampFactory, _currentTransactionId, after: createdLink));
            return createdLinkIndex;
        }

        public void SetLinkValue(IList<ulong> parts)
        {
            var beforeLink = new UInt64Link(_linksMemoryManager.GetLinkValue(parts[Constants.IndexPart]));
            _linksMemoryManager.SetLinkValue(parts);
            var afterLink = new UInt64Link(_linksMemoryManager.GetLinkValue(parts[Constants.IndexPart]));
            CommitTransition(new Transition(_uniqueTimestampFactory, _currentTransactionId, before: beforeLink, after: afterLink));
        }

        public void FreeLink(ulong link)
        {
            var deletedLink = new UInt64Link(_linksMemoryManager.GetLinkValue(link));
            _linksMemoryManager.FreeLink(link);
            CommitTransition(new Transition(_uniqueTimestampFactory, _currentTransactionId, before: deletedLink));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Queue<Transition> GetCurrentTransitions()
        {
            return _currentTransactionTransitions ?? _transitions;
        }

        private void CommitTransition(Transition transition)
        {
            if (_currentTransaction != null) Transaction.EnsureTransactionAllowsWriteOperations(_currentTransaction);
            var transitions = GetCurrentTransitions();
            transitions.Enqueue(transition);
        }

        private void RevertTransition(Transition transition)
        {
            if (transition.After.IsNull()) // Revert Deletion with Creation
                _linksMemoryManager.AllocateLink();
            else if (transition.Before.IsNull()) // Revert Creation with Deletion
                _linksMemoryManager.FreeLink(transition.After.Index);
            else // Revert Update
                _linksMemoryManager.SetLinkValue(new[] { transition.After.Index, transition.Before.Source, transition.Before.Target });
        }

        private void ResetCurrentTransation()
        {
            _currentTransactionId = 0;
            _currentTransactionTransitions = null;
            _currentTransaction = null;
        }

        private void PushTransitions()
        {
            if (_log == null || _transitions == null) return;

            for (var i = 0; i < _transitions.Count; i++)
            {
                var transition = _transitions.Dequeue();

                _log.Write(transition);
                _lastCommitedTransition = transition;
            }
        }

        private void TransitionsPusher()
        {
            while (!IsDisposed && _transitionsPusher != null)
            {
                Thread.Sleep(DefaultPushDelay);
                PushTransitions();
            }
        }

        public Transaction BeginTransaction()
        {
            return new Transaction(this);
        }

        private void DisposeTransitions()
        {
            try
            {
                var pusher = _transitionsPusher;
                if (pusher != null)
                {
                    _transitionsPusher = null;
                    pusher.Wait();
                }
                if (_transitions != null)
                    PushTransitions();

                Disposable.TryDispose(_log);

                FileHelpers.WriteFirst(_logAddress, _lastCommitedTransition);
            }
            catch
            {
            }
        }

        #region DisposalBase

        protected override bool AllowMultipleDisposeCalls => true;

        protected override void DisposeCore(bool manual)
        {
            if (manual)
            {
                DisposeTransitions();
                Disposable.TryDispose(_linksMemoryManager);
            }
        }

        #endregion
    }
}