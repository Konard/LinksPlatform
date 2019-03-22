using System.IO;
using System.Text;
using System.Threading;
using Platform.Data.Core.Doublets;
using Platform.Data.Core.Sequences;
using Platform.Helpers;

namespace Platform.Examples
{
    /// <remarks>
    /// https://tools.ietf.org/html/rfc4180
    /// </remarks>
    public class CSVExporter
    {
        private static readonly LinksConstants<bool, ulong, long> Constants = Default<LinksConstants<bool, ulong, long>>.Instance;

        private readonly bool _unicodeMapped;
        private readonly SynchronizedLinks<ulong> _links;

        public CSVExporter(SynchronizedLinks<ulong> links, bool unicodeMapped = false)
        {
            _links = links;
            _unicodeMapped = unicodeMapped;
        }

        public void Export(string path, CancellationToken cancellationToken)
        {
            using (var file = File.OpenWrite(path))
            using (var writer = new StreamWriter(file, Encoding.UTF8))
            {
                var links = 1;
                var lines = 0;

                _links.Each(Constants.Any, Constants.Any, linkId =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return Constants.Break;

                    if (!_unicodeMapped || links > UnicodeMap.MapSize)
                    {
                        if (lines > 0)
                            writer.WriteLine();

                        var link = new UInt64Link(_links.Unsync.GetLink(linkId)); // Use GetLinkCore only inside each (it is not thread safe).

                        writer.Write("{0},{1}", FormatLink(link.Source), FormatLink(link.Target));
                        lines++;
                    }

                    links++;
                    return Constants.Continue;
                });
            }
        }

        private string FormatLink(ulong link)
        {
            const char comma = ',';
            const char doubleQuote = '"';

            if (_unicodeMapped && link <= UnicodeMap.MapSize)
            {
                var character = UnicodeMap.FromLinkToChar(link);

                if (character == comma)
                    return "\",\"";
                if (character == doubleQuote)
                    return "\"\"\"\"";
                return character.ToString();
            }

            return link.ToString();
        }
    }
}

