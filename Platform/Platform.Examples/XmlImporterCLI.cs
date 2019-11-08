using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.ResizableDirectMemory.Generic;

namespace Platform.Examples
{
    public class XmlImporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var file = ConsoleHelpers.GetOrReadArgument(1, "Xml file", args);

            if (!File.Exists(file))
            {
                Console.WriteLine("Entered xml file does not exists.");
            }
            else
            {
                const long gb32 = 34359738368;

                using (var cancellation = new ConsoleCancellation())
                using (var memoryAdapter = new ResizableDirectMemoryLinks<uint>(linksFile, gb32))
                //using (var memoryAdapter = new UInt64ResizableDirectMemoryLinks(linksFile, gb32))
                //using (var links = new UInt64Links(memoryAdapter))
                {
                    Console.WriteLine("Press CTRL+C to stop.");
                    var links = memoryAdapter.DecorateWithAutomaticUniquenessAndUsagesResolution();
                    var indexer = new XmlIndexer<uint>(links);
                    var indexingImporter = new XmlImporter<uint>(indexer);
                    indexingImporter.Import(file, cancellation.Token).Wait();
                    if (cancellation.NotRequested)
                    {
                        var cache = indexer.Cache;
                        //var counter = new TotalSequenceSymbolFrequencyCounter<uint>(links);
                        //var cache = new LinkFrequenciesCache<uint>(links, counter);
                        Console.WriteLine("Frequencies cache ready.");
                        var storage = new LinksXmlStorage<uint>(links, false, cache);
                        var importer = new XmlImporter<uint>(storage);
                        importer.Import(file, cancellation.Token).Wait();
                    }
                }
            }
        }
    }
}
