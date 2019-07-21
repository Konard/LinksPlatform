using System.Diagnostics;

namespace Platform.Helpers.Console
{
    public static class ConsoleHelpers
    {
        public static void PressAnyKeyToContinue()
        {
            // TODO: Internationalization
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
        }

        public static string GetOrReadArgument(int index, params string[] args) => GetOrReadArgument(index, $"{index + 1} argument", args);

        public static string GetOrReadArgument(int index, string readMessage, params string[] args)
        {
            string result;

            if (args != null && args.Length > index)
                result = args[index];
            else
            {
                System.Console.Write($"{readMessage}: ");
                result = System.Console.ReadLine();
            }

            return (result ?? "").Trim().Trim('"').Trim();
        }

        [Conditional("DEBUG")]
        public static void Debug(string format, params object[] args) => System.Console.WriteLine(format, args);
    }
}
