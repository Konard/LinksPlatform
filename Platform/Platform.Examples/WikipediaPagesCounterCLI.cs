using System;
using System.IO;
using Platform.IO;

namespace Platform.Examples
{
    public class WikipediaPagesCounterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var wikipediaFile = ConsoleHelpers.GetOrReadArgument(0, "Wikipedia xml file", args);
            if (!File.Exists(wikipediaFile))
            {
                Console.WriteLine("Entered wikipedia xml file does not exists.");
            }
            else
            {
                using (var cancellation = new ConsoleCancellationHandler())
                {
                    var wikipediaPagesCounter = new WikipediaPagesCounter();
                    wikipediaPagesCounter.Count(wikipediaFile, cancellation.Token).Wait();
                }
            }
            ConsoleHelpers.PressAnyKeyToContinue();
        }
    }
}
