using System;
using System.IO;
using Platform.IO;

namespace Platform.Examples
{
    public class XmlElementCounterCLI : ICommandLineInterface
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
                using (var cancellation = new ConsoleCancellation())
                {
                    Console.WriteLine("Press CTRL+C to stop.");
                    var wikipediaPagesCounter = new XmlElementCounter();
                    wikipediaPagesCounter.Count(wikipediaFile, cancellation.Token).Wait();
                }
            }
            ConsoleHelpers.PressAnyKeyToContinue();
        }
    }
}
