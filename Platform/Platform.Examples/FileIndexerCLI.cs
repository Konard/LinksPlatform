using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.ResizableDirectMemory;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;
using Platform.Data.Doublets.Sequences.Indexes;

namespace Platform.Examples
{
    public class FileIndexerCLI : ICommandLineInterface
    {
        public void Run(string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var fileToIndex = ConsoleHelpers.GetOrReadArgument(1, "File to index", args);
            if (!File.Exists(fileToIndex))
            {
                Console.WriteLine("Entered file to index does not exists.");
            }
            else
            {
                using (var cancellation = new ConsoleCancellation())
                using (var memoryAdapter = new UInt64ResizableDirectMemoryLinks(linksFile, UInt64ResizableDirectMemoryLinks.DefaultLinksSizeStep * 16))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    Console.WriteLine("Press CTRL+C to stop.");
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    links.UseUnicode();
                    UnicodeMap.InitNew(syncLinks);
                    var index = new SequenceIndex<ulong>(syncLinks);
                    var fileIndexer = new FileIndexer(syncLinks, index);
                    //fileIndexer.IndexAsync(fileToIndex, cancellationSource.Token).Wait();
                    //fileIndexer.IndexSync(fileToIndex, cancellationSource.Token);
                    fileIndexer.IndexParallel(fileToIndex, cancellation.Token);
                }
            }
        }
    }
}
