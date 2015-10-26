using System;
using System.Runtime.CompilerServices;

namespace Platform.Helpers.Synchronization
{
    public class UnsafeSynchronization : ISyncronization
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteReadOperation(Action action)
        {
            action();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ExecuteReadOperation<T>(Func<T> func)
        {
            return func();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteWriteOperation(Action action)
        {
            action();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ExecuteWriteOperation<T>(Func<T> func)
        {
            return func();
        }
    }
}