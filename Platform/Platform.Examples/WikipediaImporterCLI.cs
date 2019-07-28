using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;

namespace Platform.Examples
{
    public class WikipediaImporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var wikipediaFile = ConsoleHelpers.GetOrReadArgument(1, "Wikipedia xml file", args);

            if (!File.Exists(linksFile))
            {
                Console.WriteLine("Entered links file does not exists.");
            }
            else if (!File.Exists(wikipediaFile))
            {
                Console.WriteLine("Entered wikipedia xml file does not exists.");
            }
            else
            {
                const long gb32 = 34359738368;

                using (var cancellation = new ConsoleCancellationHandler())
                using (var memoryAdapter = new UInt64ResizableDirectMemoryLinks(linksFile, gb32))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    links.UseUnicode();

                    var sequences = new Sequences(syncLinks, new SequencesOptions<ulong> { UseCompression = true });
                    var wikipediaStorage = new WikipediaLinksStorage(sequences);
                    var wikipediaImporter = new WikipediaImporter(wikipediaStorage);

                    wikipediaImporter.Import(wikipediaFile, cancellation.Token).Wait();
                }
            }

            ConsoleHelpers.PressAnyKeyToContinue();
        }
    }
}
