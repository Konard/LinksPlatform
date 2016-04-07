using System;
using System.IO;
using Platform.Data.Core.Pairs;
using Platform.Helpers;

namespace Platform.Sandbox
{
    public class CSVExporterCLI
    {
        public void Run(string[] args)
        {
            var linksFile = ConsoleHelpers.GetOrReadArgument(0, "Links file", args);
            var exportTo = ConsoleHelpers.GetOrReadArgument(1, "Export to", args);
            var unicodeMapped = ConsoleHelpers.GetOrReadArgument(1, "Unicode is mapped", args);

            File.Create(exportTo).Dispose();

            if (!File.Exists(linksFile))
                Console.WriteLine("Entered links file does not exists.");
            else if (!File.Exists(exportTo))
                Console.WriteLine("Entered exported file cannot be created.");
            else
            {
                var cancellationSource = ConsoleHelpers.HandleCancellation();

                using (var memoryManager = new LinksMemoryManager(linksFile))
                using (var links = new Links(memoryManager))
                {
                    bool isUnicodeMapped;
                    bool.TryParse(unicodeMapped, out isUnicodeMapped);

                    var exporter = new CSVExporter(links, isUnicodeMapped);

                    exporter.Export(exportTo, cancellationSource.Token);
                }
            }

            ConsoleHelpers.PressAnyKeyToContinue();
        }
    }
}
