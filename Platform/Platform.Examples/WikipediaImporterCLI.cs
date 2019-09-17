using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.ResizableDirectMemory;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    public class WikipediaImporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var wikipediaFile = ConsoleHelpers.GetOrReadArgument(1, "Wikipedia xml file", args);

            if (!File.Exists(wikipediaFile))
            {
                Console.WriteLine("Entered wikipedia xml file does not exists.");
            }
            else
            {
                const long gb32 = 34359738368;

                using (var cancellation = new ConsoleCancellationHandler())
                using (var memoryAdapter = new ResizableDirectMemoryLinks<uint>(linksFile, gb32))
                //using (var memoryAdapter = new UInt64ResizableDirectMemoryLinks(linksFile, gb32))
                //using (var links = new UInt64Links(memoryAdapter))
                {
                    var links = memoryAdapter.DecorateWithAutomaticUniquenessAndUsagesResolution();
                    var wikipediaIndexer = new WikipediaIndexer<uint>(links);
                    var wikipediaIndexingImporter = new WikipediaImporter<uint>(wikipediaIndexer);
                    wikipediaIndexingImporter.Import(wikipediaFile, cancellation.Token).Wait();
                    if (!cancellation.IsCancellationRequested)
                    {
                        var cache = wikipediaIndexer.Cache;
                        Console.WriteLine("Frequencies cache ready.");
                        var wikipediaStorage = new WikipediaLinksStorage<uint>(links, cache);
                        var wikipediaImporter = new WikipediaImporter<uint>(wikipediaStorage);
                        wikipediaImporter.Import(wikipediaFile, cancellation.Token).Wait();
                    }
                }
            }

            ConsoleHelpers.PressAnyKeyToContinue();
        }
    }
}
