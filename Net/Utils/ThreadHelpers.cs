using System.Threading;

namespace Utils
{
    public static class ThreadHelpers
    {
        public static void SyncInvokeWithExtendedStack<T>(T param, ParameterizedThreadStart action, int maxStackSize = 200 * 1024 * 1024)
        {
            var thread = new Thread(action, maxStackSize);
            thread.Start(param);
            thread.Join();
        }

        public static void SyncInvokeWithExtendedStack(ThreadStart action, int maxStackSize = 200 * 1024 * 1024)
        {
            var thread = new Thread(action, maxStackSize);
            thread.Start();
            thread.Join();
        }
    }
}
