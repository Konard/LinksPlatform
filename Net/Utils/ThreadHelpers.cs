using System.Threading;

namespace Utils
{
    static public class ThreadHelpers
    {
        static public void SyncInvokeWithExtendedStack<T>(T param, ParameterizedThreadStart action, int maxStackSize = 200 * 1024 * 1024)
        {
            var thread = new Thread(action, maxStackSize);
            thread.Start(param);
            thread.Join();
        }

        static public void SyncInvokeWithExtendedStack(ThreadStart action, int maxStackSize = 200 * 1024 * 1024)
        {
            var thread = new Thread(action, maxStackSize);
            thread.Start();
            thread.Join();
        }
    }
}
