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
#if DEBUG
            catch (Exception ex)
            {
                // TODO: Log exception

                throw ex;
            }
#endif
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
#if DEBUG
            catch (Exception ex)
            {
                // TODO: Log exception

                throw ex;
            }
#endif
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
#if DEBUG
            catch (Exception ex)
            {
                // TODO: Log exception

                throw ex;
            }
#endif
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
#if DEBUG
            catch (Exception ex)
            {
                // TODO: Log exception

                throw ex;
            }
#endif
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
    }
}