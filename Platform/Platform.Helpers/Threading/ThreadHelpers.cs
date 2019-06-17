using System;
using System.Threading;

namespace Platform.Helpers.Threading
{
    public static class ThreadHelpers
    {
        public static void SyncInvokeWithExtendedStack<T>(T param, Action<object> action, int maxStackSize = 200 * 1024 * 1024)
        {
#if DEBUG
            var thread = new Thread(obj =>
            {
                try
                {
                    action(obj);
                }
                catch (Exception exception)
                {
                    Global.OnIgnoredException(exception);
                }
            }, maxStackSize);
#else
            var thread = new Thread(new ParameterizedThreadStart(action), maxStackSize);
#endif
            thread.Start(param);
            thread.Join();
        }

        public static void SyncInvokeWithExtendedStack(Action action, int maxStackSize = 200 * 1024 * 1024)
        {
#if DEBUG
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    Global.OnIgnoredException(exception);
                }
            }, maxStackSize);
#else
            var thread = new Thread(new ThreadStart(action), maxStackSize);
#endif
            thread.Start();
            thread.Join();
        }

        public static Thread StartNew<T>(T param, Action<object> action)
        {
#if DEBUG
            var thread = new Thread(obj =>
            {
                try
                {
                    action(obj);
                }
                catch (Exception exception)
                {
                    Global.OnIgnoredException(exception);
                }
            });
#else
            var thread = new Thread(new ParameterizedThreadStart(action));
#endif
            thread.Start(param);
            return thread;
        }

        public static Thread StartNew(Action action)
        {
#if DEBUG
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    Global.OnIgnoredException(exception);
                }
            });
#else
            var thread = new Thread(new ThreadStart(action));
#endif
            thread.Start();
            return thread;
        }

        public static void Sleep() => Thread.Sleep(1);
    }
}
