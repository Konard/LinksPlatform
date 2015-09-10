using System;
using System.Threading;

namespace Platform.Links.System.Helpers.Synchronization
{
    public class SafeSynchronization : ISyncronization
    {
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public void ExecuteReadOperation(Action action)
        {
            try
            {
                _rwLock.EnterReadLock();

                action();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public T ExecuteReadOperation<T>(Func<T> func)
        {
            try
            {
                _rwLock.EnterReadLock();

                return func();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public void ExecuteWriteOperation(Action action)
        {
            try
            {
                _rwLock.EnterWriteLock();

                action();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public T ExecuteWriteOperation<T>(Func<T> func)
        {
            try
            {
                _rwLock.EnterWriteLock();

                return func();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
    }
}
