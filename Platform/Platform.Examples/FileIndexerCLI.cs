using System;
using System.IO;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;
using Platform.Helpers;

namespace Platform.Examples
{
    public class FileIndexerCLI
    {
        public void Run(string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var fileToIndex = ConsoleHelpers.GetOrReadArgument(1, "File to index", args);

            if (!File.Exists(linksFile))
                Console.WriteLine("Entered links file does not exists.");
            else if (!File.Exists(fileToIndex))
                Console.WriteLine("Entered file to index does not exists.");
            else
            {
                var cancellationSource = ConsoleHelpers.HandleCancellation();

                using (var memoryManager = new UInt64LinksMemoryManager(linksFile, UInt64LinksMemoryManager.DefaultLinksSizeStep * 16))
                using (var links = new UInt64Links(memoryManager))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    UnicodeMap.InitNew(syncLinks);
                    var sequences = new Sequences(syncLinks);

                    var fileIndexer = new FileIndexer(syncLinks, sequences);

                    //fileIndexer.IndexAsync(fileToIndex, cancellationSource.Token).Wait();
                    fileIndexer.IndexSync(fileToIndex, cancellationSource.Token);
                }
            }

            ConsoleHelpers.PressAnyKeyToContinue();
        }
    }
}
