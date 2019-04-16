﻿using System;
using System.IO;
using Platform.Data.Core.Doublets;
using Platform.Data.Core.Sequences;
using Platform.Helpers;

namespace Platform.Examples
{
    public class FileIndexerCLI : ICommandLineInterface
    {
        public void Run(string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var fileToIndex = ConsoleHelpers.GetOrReadArgument(1, "File to index", args);

            if (!File.Exists(fileToIndex))
                Console.WriteLine("Entered file to index does not exists.");
            else
            {
                var cancellationSource = ConsoleHelpers.HandleCancellation();

                using (var memoryAdapter = new UInt64ResizableDirectMemoryLinks(linksFile, UInt64ResizableDirectMemoryLinks.DefaultLinksSizeStep * 16))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    UnicodeMap.InitNew(syncLinks);
                    var sequences = new Sequences(syncLinks);

                    var fileIndexer = new FileIndexer(syncLinks, sequences);

                    //fileIndexer.IndexAsync(fileToIndex, cancellationSource.Token).Wait();
                    //fileIndexer.IndexSync(fileToIndex, cancellationSource.Token);
                    fileIndexer.IndexParallel(fileToIndex, cancellationSource.Token);
                }
            }

            ConsoleHelpers.PressAnyKeyToContinue();
        }
    }
}
