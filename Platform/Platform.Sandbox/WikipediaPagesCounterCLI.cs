using System;
using System.IO;
using Platform.Helpers;

namespace Platform.Sandbox
{
    public class WikipediaPagesCounterCLI
    {
        public void Run(string[] args)
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
