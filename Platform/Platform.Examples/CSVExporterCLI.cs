using System;
using System.IO;
using Platform.IO;
using Platform.Data.Core.Doublets;

namespace Platform.Examples
{
    public class CSVExporterCLI<TExporter> : ICommandLineInterface
        where TExporter : CSVExporter, new()
    {
        public void Run(params string[] args)
        {
            var i = 0;
            var linksFile = ConsoleHelpers.GetOrReadArgument(i++, "Links file", args);
            var exportTo = ConsoleHelpers.GetOrReadArgument(i++, "Export to", args);
            var unicodeMapped = ConsoleHelpers.GetOrReadArgument(i++, "Unicode is mapped", args);
            var convertUnicodeLinksToCharacters = ConsoleHelpers.GetOrReadArgument(i++, "Convert each unicode-link to a corresponding character", args);
            var referenceByLines = ConsoleHelpers.GetOrReadArgument(i++, "Reference by row (line) number", args);

            bool.TryParse(unicodeMapped, out bool isUnicodeMapped);
            bool.TryParse(convertUnicodeLinksToCharacters, out bool doConvertUnicodeLinksToCharacters);
            bool.TryParse(referenceByLines, out bool doReferenceByLines);

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
                using (var cancellation = new ConsoleCancellationHandler())
                using (var memoryAdapter = new UInt64ResizableDirectMemoryLinks(linksFile))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);

                    var exporter = new TExporter();

                    exporter.Export(syncLinks, exportTo, isUnicodeMapped, doConvertUnicodeLinksToCharacters, doReferenceByLines, cancellation.Token);
                }
            }

            ConsoleHelpers.PressAnyKeyToContinue();
        }
    }
}