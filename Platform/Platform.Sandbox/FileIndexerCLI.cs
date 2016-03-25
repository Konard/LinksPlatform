using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;
using Platform.Helpers;

namespace Platform.Sandbox
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

                using (var memoryManager = new LinksMemoryManager(linksFile, LinksMemoryManager.DefaultLinksSizeStep * 16))
                using (var links = new Links(memoryManager))
                {
                    UnicodeMap.InitNew(links);
                    var sequences = new Sequences(links);

                    var fileIndexer = new FileIndexer(links, sequences);

                    fileIndexer.IndexAsync(fileToIndex, cancellationSource.Token).Wait();
                }
            }

            ConsoleHelpers.PressAnyKeyToContinue();
        }
    }
}
