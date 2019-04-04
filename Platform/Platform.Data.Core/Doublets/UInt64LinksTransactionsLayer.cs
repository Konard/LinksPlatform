using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Platform.Helpers;
using Platform.Helpers.Disposables;
using Platform.Helpers.IO;

namespace Platform.Data.Core.Doublets
{
    public class UInt64LinksTransactionsLayer : LinksDisposableDecoratorBase<ulong>
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
            // TODO: Возможно точнее будет хранить не только Source и Target, но и Index
            public UInt64Link Before;
            public UInt64Link After;
            public UniqueTimestamp Timestamp;

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
            private static readonly ConcurrentDictionary<UInt64LinksTransactionsLayer, ReaderWriterLockSlim> TransactionLocks = new ConcurrentDictionary<UInt64LinksTransactionsLayer, ReaderWriterLockSlim>();

            private readonly ConcurrentQueue<Transition> _transitions;
            private readonly UInt64LinksTransactionsLayer _layer;
            private readonly ReaderWriterLockSlim _lock;
            private bool _commited;
            private bool _reverted;

            public Transaction(UInt64LinksTransactionsLayer layer)
            {
                _lock = TransactionLocks.GetOrAdd(layer, x => new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion));
                _lock.EnterWriteLock(); // Самый простой случай: разрешать только 1 транзакцию базу данных

                _layer = layer;

                _commited = false;
                _reverted = false;

                _transitions = new ConcurrentQueue<Transition>();

                // Set Current Transaction
                _layer._currentTransactionId = _layer._lastCommitedTransition.TransactionId + 1;
                _layer._currentTransactionTransitions = _transitions;
            }

            public void Commit()
            {
                if (_reverted) throw new InvalidOperationException("Transation already reverted.");
                if (_commited) throw new InvalidOperationException("Transation already commited.");

                while (_transitions.Count > 0)
                {
                    Transition transition;
                    if (_transitions.TryDequeue(out transition))
                        _layer._transitions.Enqueue(transition);
                }

                _commited = true;
            }

            private void Revert()
            {
                if (_reverted) throw new InvalidOperationException("Transation already reverted.");
                if (_commited) throw new InvalidOperationException("Transation already commited.");

                _layer.ResetCurrentTransation(); // !

                var transitionsToRevert = new Transition[_transitions.Count];
                _transitions.CopyTo(transitionsToRevert, 0);

                for (var i = transitionsToRevert.Length - 1; i >= 0; i--)
                    _layer.RevertTransition(transitionsToRevert[i]);

                _reverted = true;
            }

            protected override void DisposeCore(bool manual)
            {
                if (manual)
                {
                    if (_layer != null && !_layer.IsDisposed)
                    {
                        if (!_commited && !_reverted)
                            Revert();
                        else
                            _layer.ResetCurrentTransation(); // !
                    }

                    _lock.ExitWriteLock();
                }
            }
        }

        private static readonly TimeSpan DefaultPushDelay = TimeSpan.FromSeconds(0.1);

        private readonly string _logAddress;
        private readonly FileStream _binaryLogger;
        private readonly ConcurrentQueue<Transition> _transitions;
        private readonly UniqueTimestampFactory _uniqueTimestampFactory;
        private Task _transitionsPusher;
        private Transition _lastCommitedTransition;
        private ulong _currentTransactionId;
        private ConcurrentQueue<Transition> _currentTransactionTransitions;

        public UInt64LinksTransactionsLayer(ILinks<ulong> links, string logAddress)
            : base(links)
        {
            if (string.IsNullOrWhiteSpace(logAddress))
                throw new ArgumentNullException(nameof(logAddress));

            // В первой строке файла хранится последняя закоммиченную транзакцию.
            // При запуске это используется для проверки удачного закрытия файла лога.

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

            _uniqueTimestampFactory = new UniqueTimestampFactory();

            _logAddress = logAddress;
            _binaryLogger = FileHelpers.Append(logAddress);
            _transitions = new ConcurrentQueue<Transition>();
            _transitionsPusher = new Task(TransitionsPusher);
            _transitionsPusher.Start();
        }

        public virtual ulong Create() 
        { 
            var createdLinkIndex = Links.Create();
            var createdLink = new UInt64Link(Links.GetLink(createdLinkIndex));
            CommitTransition(new Transition { TransactionId = _currentTransactionId, After = createdLink });
            return createdLinkIndex;
        }

        public virtual ulong Update(IList<ulong> restrictions) 
        {
            if (restrictions.Count == 3)
            {
                var beforeLink = new UInt64Link(Links.GetLink(restrictions[Constants.IndexPart]));
                var updatedLinkIndex = Links.Update(restrictions);
                var afterLink = new UInt64Link(Links.GetLink(updatedLinkIndex));
                CommitTransition(new Transition { TransactionId = _currentTransactionId, Before = beforeLink, After = afterLink });
                return updatedLinkIndex;
            }
            else
                return Links.Update(restrictions);
        } 

        public virtual void Delete(ulong link) 
        {
            var deletedLink = new UInt64Link(Links.GetLink(link));
            Links.Delete(link);
            CommitTransition(new Transition { TransactionId = _currentTransactionId, Before = deletedLink });
        } 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ConcurrentQueue<Transition> GetCurrentTransitions()
        {
            return _currentTransactionTransitions ?? _transitions;
        }

        private void CommitTransition(Transition transition)
        {
            var transitions = GetCurrentTransitions();
            if (transitions != null)
                transitions.Enqueue(transition);
        }

        private void RevertTransition(Transition transition)
        {
            if (transition.After.IsNull()) // Revert Deletion with Creation
            {
                var source = transition.Before.Source;
                var target = transition.Before.Target;

                EnsureSelfReferencingLinkIsRestored(source, target);

                Links.CreateAndUpdate(source, target);
            }
            else if (transition.Before.IsNull()) // Revert Creation with Deletion
                this.DeleteIfExists(transition.After.Source, transition.After.Target);
            else // Revert Update
            {
                var beforeSource = transition.Before.Source;
                var beforeTarget = transition.Before.Target;

                EnsureSelfReferencingLinkIsRestored(beforeSource, beforeTarget);

                // TODO: Проверить корректно ли это теперь работает
                Links.UpdateOrCreateOrGet(transition.After.Source, transition.After.Target, beforeSource, beforeTarget);
            }
        }

        private void EnsureSelfReferencingLinkIsRestored(ulong source, ulong target)
        {
            // Возможно эту логику нужно перенисти в функцию Create
            if (Links.Count(source) == 0 && Links.Count(target) == 0 && source == target)
            {
                if (Create() != source)
                    throw new Exception("Невозможно восстановить связь");
            }
            else if (Links.Count(target) == 0)
            {
                if (Links.CreateAndUpdate(source, Constants.Itself) != target)
                    throw new Exception("Невозможно восстановить связь");
            }
            else if (Links.Count(source) == 0)
            {
                if (Links.CreateAndUpdate(Constants.Itself, target) != source)
                    throw new Exception("Невозможно восстановить связь");
            }
        }

        private void ResetCurrentTransation()
        {
            _currentTransactionId = 0;
            _currentTransactionTransitions = null;
        }

        private void PushTransitions()
        {
            if (_binaryLogger == null || _transitions == null) return;

            var amountToLog = _transitions.Count;
            for (var i = 0; i < amountToLog; i++)
            {
                Transition transition;
                if (!_transitions.TryDequeue(out transition))
                    return;

                transition.Timestamp = _uniqueTimestampFactory.Create();

                _binaryLogger.Write(transition);
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
                if (_binaryLogger != null)
                    _binaryLogger.Dispose();

                if (!string.IsNullOrWhiteSpace(_logAddress))
                    FileHelpers.WriteFirst(_logAddress, _lastCommitedTransition);
            }
            catch
            {
            }
        }

        #region DisposalBase

        protected override void DisposeCore(bool manual)
        {
            if (manual)
            {
                DisposeTransitions();
                Disposable.TryDispose(Links);
            }
        }

        #endregion
    }
}