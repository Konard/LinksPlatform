#define LinksTransactions

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Platform.Links.System.Helpers;

#if LinksTransactions

namespace Platform.Links.DataBase.CoreUnsafe.Pairs
{
    public partial class Links
    {
        public struct Transition
        {
            // TODO: Реализовать механизм Транзакций (Commit'ов) c возможностью откатов
            public ulong TransactionId; // 8 байт
            public Structures.Link Before; // 8 * 3 = 24 байт
            public Structures.Link After; // 8 * 3 = 24 байт

            public override string ToString()
            {
                return string.Format("{0}: {1} => {2}", TransactionId, Before, After);
            }
        }

        private static readonly TimeSpan DefaultPushDelay = TimeSpan.FromSeconds(0.5);

        private readonly FileStream _binaryLogger;
        private readonly ConcurrentQueue<Transition> _transitions;
        private Task _transitionsPusher;

        public Links(string address, string logAddress, long size)
            : this(address, size)
        {
            _binaryLogger = FileHelpers.Append(logAddress);
            _transitions = new ConcurrentQueue<Transition>();
            _transitionsPusher = new Task(TransitionsPusher);
            _transitionsPusher.Start();
        }

        private void CommitCreation(Structures.Link after)
        {
            if (_transitions != null)
                _transitions.Enqueue(new Transition { After = after });
        }

        private void CommitUpdate(Structures.Link before, Structures.Link after)
        {
            if (_transitions != null)
                _transitions.Enqueue(new Transition { Before = before, After = after });
        }

        private void CommitDeletion(Structures.Link before)
        {
            if (_transitions != null)
                _transitions.Enqueue(new Transition { Before = before });
        }

        private void CommitTransition(Transition transition)
        {
            if (_transitions != null)
                _transitions.Enqueue(transition);
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
                _binaryLogger.Write(transition);
            }
        }

        private void TransitionsPusher()
        {
            while (!_disposed && _transitionsPusher != null)
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
                    pusher.Dispose();
                }
                if (_transitions != null)
                {
                    CommitTransition(new Transition()); // Mark successful shutdown
                    PushTransitions();
                }
                if (_binaryLogger != null)
                    _binaryLogger.Dispose();
            }
            finally
            {
            }
        }

        // TODO: Будут ли работать эти функции как ожидается?

        public void EnterTransaction()
        {
            //_rwLock.EnterWriteLock();
        }

        public void ExitTransaction()
        {
            //_rwLock.ExitWriteLock();
        }
    }
}

#endif