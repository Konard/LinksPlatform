using System;

namespace Platform.Helpers.Synchronization
{
    public class UnsafeSynchronization : ISyncronization
    {
        public void ExecuteReadOperation(Action action)
        {
            action();
        }

        public T ExecuteReadOperation<T>(Func<T> func)
        {
            return func();
        }

        public void ExecuteWriteOperation(Action action)
        {
            action();
        }

        public T ExecuteWriteOperation<T>(Func<T> func)
        {
            return func();
        }
    }
}