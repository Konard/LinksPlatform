using System;

#if NET45
using System.Threading;
#endif

namespace Platform.Helpers.Threading
{
    public static class ThreadHelpers
    {
        public static void SyncInvokeWithExtendedStack<T>(T param, Action<object> action, int maxStackSize = 200 * 1024 * 1024)
        {
#if NET45
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
#else
            action(param);
#endif
        }

        public static void SyncInvokeWithExtendedStack(Action action, int maxStackSize = 200 * 1024 * 1024)
        {
#if NET45
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
#else
            action();
#endif
        }

#if NET45
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
#endif
    }
}
