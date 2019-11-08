using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.ResizableDirectMemory.Specific;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    public class GEXFCSVExporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var exportTo = ConsoleHelpers.GetOrReadArgument(1, "Export to", args);
            File.Create(exportTo).Dispose();
            if (!File.Exists(linksFile))
            {
                Console.WriteLine("Entered links file does not exists.");
            }
            else if (!File.Exists(exportTo))
            {
                Console.WriteLine("Entered exported file cannot be created.");
            }
            else
            {
                using (var memoryAdapter = new UInt64ResizableDirectMemoryLinks(linksFile))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    var exporter = new GEXFExporter<ulong>(syncLinks);
                    exporter.Export(exportTo);
                }
            }
        }
    }
}
