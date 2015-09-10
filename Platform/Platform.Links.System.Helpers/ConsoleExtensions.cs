using System;

namespace Platform.Links.System.Helpers
{
    /// <summary>
    /// Представляет класс-контейнер методов-расширений для класса System.Console.
    /// </summary>
    public static class ConsoleExtensions
    {
        /// <summary>
        /// Разделитель используемый при печати в консоль.
        /// </summary>
        public static string Separator = "---";

        /// <summary>Печатает исключение к консоль.</summary>
        /// <param name="ex">Исключение, которое будет напечатано в консоли.</param>
        public static void WriteToConsole(this Exception ex)
        {
            Console.Write("Exception: ");
            Console.WriteLine(ex.Message);

            Console.WriteLine(Separator);

            if (ex.InnerException != null)
            {
                Console.WriteLine("Inner Exception: ");

                ex.InnerException.WriteToConsole();
            }

            Console.WriteLine(Separator);

            Console.WriteLine(ex.StackTrace);
        }
    }
}