using System;

namespace Platform.Helpers.Threading
{
    public interface ISyncronization
    {
        void ExecuteReadOperation(Action action);

        T ExecuteReadOperation<T>(Func<T> func);

        void ExecuteWriteOperation(Action action);

        T ExecuteWriteOperation<T>(Func<T> func);
    }
}