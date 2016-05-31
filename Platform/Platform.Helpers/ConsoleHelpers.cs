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

        public static string GetOrReadArgument(int index, params string[] args) => GetOrReadArgument(index, $"{index + 1} argument");

        public static string GetOrReadArgument(int index, string readMessage, params string[] args)
        {
            string result;

            if (args != null && args.Length > index)
                result = args[index];
            else
            {
                Console.Write($"{readMessage}: ");
                result = Console.ReadLine();
            }

            return (result ?? "").Trim().Trim('"').Trim();
        }

        [Conditional("DEBUG")]
        public static void Debug(string format, params object[] args) => Console.WriteLine(format, args);

        [Conditional("DEBUG")]
        public static void EnableIgnoredExceptionsAutoPrint() => Global.IgnoredException += OnIgnoredException;

        [Conditional("DEBUG")]
        public static void DisableIgnoredExceptionsAutoPrint() => Global.IgnoredException -= OnIgnoredException;

        private static void OnIgnoredException(object sender, Exception e) => Console.WriteLine(e.ToRecursiveString());
    }
}
