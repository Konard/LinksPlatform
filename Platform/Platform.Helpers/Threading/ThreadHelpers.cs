using System;
using System.Threading;

namespace Platform.Helpers.Threading
{
    public static class ThreadHelpers
    {
        public const int DefaultMaxStackSize = 0;
        public const int DefaultSleepTimeout = 1;
        public const int ExtendedMaxStackSize = 200 * 1024 * 1024;

        public static void SyncInvokeWithExtendedStack<T>(T param, Action<object> action, int maxStackSize = ExtendedMaxStackSize) => StartNew(param, action, maxStackSize).Join();

        public static void SyncInvokeWithExtendedStack(Action action, int maxStackSize = ExtendedMaxStackSize) => StartNew(action, maxStackSize).Join();

        public static Thread StartNew<T>(T param, Action<object> action, int maxStackSize = DefaultMaxStackSize)
        {
            var thread = new Thread(new ParameterizedThreadStart(action), maxStackSize);
            thread.Start(param);
            return thread;
        }

        public static Thread StartNew(Action action, int maxStackSize = DefaultMaxStackSize)
        {
            var thread = new Thread(new ThreadStart(action), maxStackSize);
            thread.Start();
            return thread;
        }

        public static void Sleep() => Thread.Sleep(DefaultSleepTimeout);
    }
}
