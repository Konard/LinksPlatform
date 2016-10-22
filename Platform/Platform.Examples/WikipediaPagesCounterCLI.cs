using System;
using System.IO;
using Platform.Helpers;

namespace Platform.Examples
{
    public class WikipediaPagesCounterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var wikipediaFile = ConsoleHelpers.GetOrReadArgument(0, "Wikipedia xml file", args);

            if (!File.Exists(wikipediaFile))
                Console.WriteLine("Entered wikipedia xml file does not exists.");
            else
            {
                var cancellationSource = ConsoleHelpers.HandleCancellation();

                var wikipediaPagesCounter = new WikipediaPagesCounter();
                wikipediaPagesCounter.Count(wikipediaFile, cancellationSource.Token).Wait();
            }

            ConsoleHelpers.PressAnyKeyToContinue();
        }
    }
}
