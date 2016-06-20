using System;
using System.IO;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;
using Platform.Helpers;

namespace Platform.Sandbox
{
    public class WikipediaImporterCLI
    {
        public void Run(string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var wikipediaFile = ConsoleHelpers.GetOrReadArgument(1, "Wikipedia xml file", args);

            if (!File.Exists(linksFile))
                Console.WriteLine("Entered links file does not exists.");
            else if (!File.Exists(wikipediaFile))
                Console.WriteLine("Entered wikipedia xml file does not exists.");
            else
            {
                var cancellationSource = ConsoleHelpers.HandleCancellation();

                using (var memoryManager = new UInt64LinksMemoryManager(linksFile, UInt64LinksMemoryManager.DefaultLinksSizeStep * 16))
                
                using (var links = new UInt64Links(memoryManager))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    UnicodeMap.InitNew(links);
                    var sequences = new Sequences(syncLinks);
                    var wikipediaStorage = new WikipediaLinksStorage(sequences);
                    var wikipediaImporter = new WikipediaImporter(wikipediaStorage);

                    wikipediaImporter.Import(wikipediaFile, cancellationSource.Token).Wait();
                }
            }

            ConsoleHelpers.PressAnyKeyToContinue();
        }
    }
}
