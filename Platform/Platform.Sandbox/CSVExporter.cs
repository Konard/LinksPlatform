using System.IO;
using System.Text;
using System.Threading;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;

namespace Platform.Sandbox
{
    /// <remarks>
    /// https://tools.ietf.org/html/rfc4180
    /// </remarks>
    public class CSVExporter
    {
        private readonly bool _unicodeMapped;
        private readonly Links _links;

        public CSVExporter(Links links, bool unicodeMapped = false)
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

                _links.Each(LinksConstants.Any, LinksConstants.Any, linkId =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return LinksConstants.Break;

                    if (!_unicodeMapped || links > UnicodeMap.MapSize)
                    {
                        if (lines > 0)
                            writer.Write("\r\n");

                        var link = _links.GetLinkCore(linkId); // Use GetLinkCore only inside each (it is not thread safe).

                        writer.Write("{0},{1}", FormatLink(link.Source), FormatLink(link.Target));
                        lines++;
                    }

                    links++;
                    return LinksConstants.Continue;
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

