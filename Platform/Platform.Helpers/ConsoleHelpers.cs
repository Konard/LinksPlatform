using System;
using System.Diagnostics;
using System.Threading;

namespace Platform.Helpers
{
    public static class ConsoleHelpers
    {
        public static void PressAnyKeyToContinue()
        {
            // TODO: Internationalization
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }

        public static CancellationTokenSource HandleCancellation()
        {
            Console.WriteLine("Press CTRL+C to stop.");

            var importCancellationSource = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, args) =>
            {
                args.Cancel = true;

                if (!importCancellationSource.IsCancellationRequested)
                {
                    importCancellationSource.Cancel();
                    Console.WriteLine("Stopping...");
                }
            };

            return importCancellationSource;
        }

        public static string GetOrReadArgument(int index, params string[] args)
        {
            return GetOrReadArgument(index, string.Format("{0} argument", index + 1));
        }

        public static string GetOrReadArgument(int index, string readMessage, params string[] args)
        {
            string result;

            if (args != null && args.Length > index)
                result = args[index];
            else
            {
                Console.Write("{0}: ", readMessage);
                result = Console.ReadLine();
            }

            return (result ?? "").Trim().Trim('"').Trim();
        }

        [Conditional("DEBUG")]
        public static void Debug(string format, params object[] args)
        {
#if DEBUG
            Console.WriteLine(format, args);
#endif
        }
    }
}
