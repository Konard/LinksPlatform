#define LinksTransactions

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Platform.Helpers;
using Platform.Helpers.Disposables;
using Platform.Helpers.IO;

#if LinksTransactions

namespace Platform.Data.Core.Pairs
{
    public partial class UInt64Links
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
            public static readonly long Size = Marshal.SizeOf(typeof(Transition));

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
            private static readonly ConcurrentDictionary<UInt64Links, ReaderWriterLockSlim> TransactionLocks = new ConcurrentDictionary<UInt64Links, ReaderWriterLockSlim>();

            private readonly ConcurrentQueue<Transition> _transitions;
            private readonly UInt64Links _links;
            private readonly ReaderWriterLockSlim _lock;
            private bool _commited;
            private bool _reverted;

            public Transaction(UInt64Links links)
            {
                _lock = TransactionLocks.GetOrAdd(links, x => new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion));
                _lock.EnterWriteLock(); // Самый простой случай: разрешать только 1 транзакцию базу данных

                _links = links;

                _commited = false;
                _reverted = false;

                _transitions = new ConcurrentQueue<Transition>();

                // Set Current Transaction
                _links._currentTransactionId = _links._lastCommitedTransition.TransactionId + 1;
                _links._currentTransactionTransitions = _transitions;
            }

            public void Commit()
            {
                if (_reverted) throw new InvalidOperationException("Transation already reverted.");
                if (_commited) throw new InvalidOperationException("Transation already commited.");

                while (_transitions.Count > 0)
                {
                    Transition transition;
                    if (_transitions.TryDequeue(out transition))
                        _links._transitions.Enqueue(transition);
                }

                _commited = true;
            }

            private void Revert()
            {
                if (_reverted) throw new InvalidOperationException("Transation already reverted.");
                if (_commited) throw new InvalidOperationException("Transation already commited.");

                if (!_commited)
                    _links._ignoreTransitions = true;

                _links.ResetCurrentTransation(); // !

                var transitionsToRevert = new Transition[_transitions.Count];
                _transitions.CopyTo(transitionsToRevert, 0);

                for (var i = transitionsToRevert.Length - 1; i >= 0; i--)
                    _links.RevertTransition(transitionsToRevert[i]);

                if (!_commited)
                    _links._ignoreTransitions = false;

                _reverted = true;
            }

            protected override void DisposeCore(bool manual)
            {
                if (manual)
                {
                    if (_links != null && !_links.IsDisposed)
                    {
                        if (!_commited && !_reverted)
                            Revert();
                        else
                            _links.ResetCurrentTransation(); // !
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
        private bool _ignoreTransitions;

        // TODO: Переосмыслить как включать/выключать транзакции
        public UInt64Links(ILinksMemoryManager<ulong> memoryManager, string logAddress)
            : this(memoryManager)
        {
            if (!string.IsNullOrWhiteSpace(logAddress))
            {
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
            else
                _ignoreTransitions = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ConcurrentQueue<Transition> GetCurrentTransitions()
        {
            return _currentTransactionTransitions ?? _transitions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CommitCreation(UInt64Link after)
        {
            CommitTransition(new Transition { TransactionId = _currentTransactionId, After = after });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CommitUpdate(UInt64Link before, UInt64Link after)
        {
            CommitTransition(new Transition { TransactionId = _currentTransactionId, Before = before, After = after });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CommitDeletion(UInt64Link before)
        {
            CommitTransition(new Transition { TransactionId = _currentTransactionId, Before = before });
        }

        private void CommitTransition(Transition transition)
        {
            if (_ignoreTransitions)
                return;

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

                this.CreateAndUpdate(source, target);
            }
            else if (transition.Before.IsNull()) // Revert Creation with Deletion
                this.DeleteIfExists(transition.After.Source, transition.After.Target);
            else // Revert Update
            {
                var beforeSource = transition.Before.Source;
                var beforeTarget = transition.Before.Target;

                EnsureSelfReferencingLinkIsRestored(beforeSource, beforeTarget);

                // TODO: Проверить корректно ли это теперь работает
                this.UpdateOrCreateOrGet(transition.After.Source, transition.After.Target, beforeSource, beforeTarget);
            }
        }

        private void EnsureSelfReferencingLinkIsRestored(ulong source, ulong target)
        {
            // Возможно эту логику нужно перенисти в функцию Create
            if (_memoryManager.Count(source) == 0 && _memoryManager.Count(target) == 0 && source == target)
            {
                if (Create() != source)
                    throw new Exception("Невозможно восстановить связь");
            }
            else if (_memoryManager.Count(target) == 0)
            {
                if (this.CreateAndUpdate(source, Constants.Itself) != target)
                    throw new Exception("Невозможно восстановить связь");
            }
            else if (_memoryManager.Count(source) == 0)
            {
                if (this.CreateAndUpdate(Constants.Itself, target) != source)
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
            catch (Exception ex)
            {
            }
        }

        public Transaction BeginTransaction()
        {
            if(string.IsNullOrWhiteSpace(_logAddress))
                throw new InvalidOperationException("Transaction log is not enabled.");
            return new Transaction(this);
        }
    }
}

#endif