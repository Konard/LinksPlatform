using System;

namespace Platform.Links.System.Helpers.Synchronization
{
    public interface ISyncronization
    {
        void ExecuteReadOperation(Action action);

        T ExecuteReadOperation<T>(Func<T> func);

        void ExecuteWriteOperation(Action action);

        T ExecuteWriteOperation<T>(Func<T> func);
    }
}