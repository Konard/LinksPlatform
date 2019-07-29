using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;
using Platform.Data;

namespace Platform.Examples
{
    /// <remarks>
    /// https://tools.ietf.org/html/rfc4180
    /// </remarks>
    public class CSVExporter
    {
        protected SynchronizedLinks<ulong> _links;
        protected bool _unicodeMapped;
        protected bool _convertUnicodeLinksToCharacters;
        protected bool _referenceByLines;
        protected HashSet<ulong> _visited;
        protected Dictionary<ulong, ulong> _addressToLineNumber = new Dictionary<ulong, ulong>();
        protected ulong _linksCounter;
        protected ulong _linesCounter;

        public void Export(SynchronizedLinks<ulong> links, string path, bool unicodeMapped, bool convertUnicodeLinksToCharacters, bool referenceByLines, CancellationToken cancellationToken)
        {
            _links = links;
            _unicodeMapped = unicodeMapped;
            _convertUnicodeLinksToCharacters = convertUnicodeLinksToCharacters;
            _referenceByLines = referenceByLines;
            _visited = new HashSet<ulong>();
            using (var file = File.OpenWrite(path))
            using (var writer = new StreamWriter(file, Encoding.UTF8))
            {
                _linksCounter = 1;
                _linesCounter = 0;
                _links.Each(link =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return _links.Constants.Break;
                    }
                    var linkIndex = link[_links.Constants.IndexPart];
                    if (!_unicodeMapped || _linksCounter > UnicodeMap.MapSize)
                    {
                        if (Visit(linkIndex))
                        {
                            WriteLink(writer, link);
                        }
                    }
                    _linksCounter++;
                    return _links.Constants.Continue;
                });
            }
        }

        protected bool Visit(ulong linkIndex)
        {
            var result = _visited.Add(linkIndex);
            if (result)
            {
                _addressToLineNumber.Add(linkIndex, (ulong)(_linesCounter + 1));
            }
            return result;
        }

        protected virtual void WriteLink(StreamWriter writer, IList<ulong> link)
        {
            writer.WriteLine($"{FormatLink(link[_links.Constants.SourcePart])},{FormatLink(link[_links.Constants.TargetPart])}");
            _linesCounter++;
        }

        protected string FormatLink(ulong link)
        {
            const char comma = ',';
            const char doubleQuote = '"';
            if (_unicodeMapped && _convertUnicodeLinksToCharacters && link <= UnicodeMap.MapSize)
            {
                var character = UnicodeMap.FromLinkToChar(link);
                if (character == comma)
                {
                    return "\",\"";
                }
                if (character == doubleQuote)
                {
                    return "\"\"\"\"";
                }
                return $"\"{character}\"";
            }
            if (_referenceByLines)
            {
                link = _addressToLineNumber[link];
            }
            return link.ToString();
        }
    }
}