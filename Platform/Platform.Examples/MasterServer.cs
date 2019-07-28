using System;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Communication.Protocol.Udp;
using Platform.Data;
using Platform.Data.Core.Doublets;
using Platform.Data.Core.Sequences;
using Platform.Helpers.Singletons;

namespace Platform.Examples
{
    public class MasterServer
    {
        private static readonly LinksConstants<bool, ulong, long> _constants = Default<LinksConstants<bool, ulong, long>>.Instance;

        private readonly ILinks<ulong> _links;
        private readonly Sequences _sequences;
        private readonly UdpSender _sender;

        public MasterServer(ILinks<ulong> links, Sequences sequences, UdpSender sender)
        {
            _links = links;
            _sequences = sequences;
            _sender = sender;
        }

        // TODO: Remove Console usage
        public void PrintContents(Action<string> messageHandler)
        {
            if (_links.Count() == UnicodeMap.LastCharLink)
            {
                messageHandler("Database is empty.");
            }
            else
            {
                messageHandler("Contents:");

                var linksTotalLength = _links.Count().ToString("0").Length;

                var printFormatBase = new string('0', linksTotalLength);

                // Выделить код по печати одной связи в Extensions

                var printFormat = string.Format("\t[{{0:{0}}}]: {{1:{0}}} -> {{2:{0}}} {{3}}", printFormatBase);

                for (var link = UnicodeMap.LastCharLink + 1; link <= _links.Count(); link++)
                {
                    messageHandler(string.Format(printFormat, link, _links.GetSource(link), _links.GetTarget(link),
                        _sequences.FormatSequence(link, AppendLinkToString, true)));
                }
            }
        }

        public bool IsSearch(string message)
        {
            var i = message.Length - 1;
            if (message[i] == '?')
            {
                var escape = 0;
                while (--i >= 0 && IsEscape(message[i]))
                {
                    escape++;
                }

                return escape % 2 == 0;
            }
            return false;
        }

        public void Create(string message)
        {
            Create(ProcessSequence(message, forSearch: false));
        }

        private void Create(ulong[] sequence)
        {
            var link = _sequences.Create(sequence);

            _sender.Send($"Sequence with balanced variant at {link} created.");
        }

        public void Search(string message)
        {
            Search(ProcessSequence(message, forSearch: true));
        }

        private void Search(ulong[] sequence)
        {
            var containsAny = Array.IndexOf(sequence, _constants.Any) >= 0;
            var containsZeroOrMany = Array.IndexOf(sequence, Sequences.ZeroOrMany) >= 0;

            if (containsZeroOrMany)
            {
                var patternMatched = _sequences.MatchPattern(sequence);

                _sender.Send($"{patternMatched.Count} sequences matched pattern.");
                foreach (var result in patternMatched)
                {
                    var sequenceMarker = _sequences.IsSequence(result) ? "[S]" : "[P]";
                    _sender.Send($"\t{result}: {_sequences.FormatSequence(result, AppendLinkToString, true)} {sequenceMarker}");
                }
            }
            if (!containsZeroOrMany)
            {
                var fullyMatched = _sequences.Each(sequence);

                _sender.Send($"{fullyMatched.Count} sequences matched fully.");
                foreach (var result in fullyMatched)
                {
                    var sequenceMarker = _sequences.IsSequence(result) ? "[S]" : "[P]";
                    _sender.Send($"\t{result}: {_sequences.FormatSequence(result, AppendLinkToString, true)} {sequenceMarker}");
                }
            }
            if (!containsAny && !containsZeroOrMany)
            {
                var partiallyMatched = _sequences.GetAllPartiallyMatchingSequences1(sequence);

                _sender.Send($"{partiallyMatched.Count} sequences matched partially.");
                foreach (var result in partiallyMatched)
                {
                    var sequenceMarker = _sequences.IsSequence(result) ? "[S]" : "[P]";
                    _sender.Send($"\t{result}: {_sequences.FormatSequence(result, AppendLinkToString, true)} {sequenceMarker}");
                }


                var allConnections = _sequences.GetAllConnections(sequence);

                _sender.Send($"{allConnections.Count} sequences connects query elements.");
                foreach (var result in allConnections)
                {
                    var sequenceMarker = _sequences.IsSequence(result) ? "[S]" : "[P]";
                    _sender.Send($"\t{result}: {_sequences.FormatSequence(result, AppendLinkToString, true)} {sequenceMarker}");
                }
            }
        }

        private ulong[] ProcessSequence(string sequence, bool forSearch = false)
        {
            var sequenceLength = sequence.Length;
            if (forSearch)
            {
                sequenceLength--;
            }

            var w = 0;
            for (var r = 0; r < sequenceLength; r++)
            {
                if (IsEscape(sequence[r])) // Last Escape Symbol Ignored
                {
                    if ((r + 1) < sequenceLength)
                    {
                        w++;
                        r++;
                    }
                }
                else
                {
                    w++;
                }
            }

            var result = new ulong[w];

            w = 0;
            if (forSearch)
            {
                for (var r = 0; r < sequenceLength; r++)
                {
                    if (IsEscape(sequence[r])) // Last Escape Symbol Ignored
                    {
                        if ((r + 1) < sequenceLength)
                        {
                            result[w++] = UnicodeMap.FromCharToLink(sequence[r + 1]);
                            r++;
                        }
                    }
                    else if (sequence[r] == '_')
                    {
                        result[w++] = _constants.Any;
                    }
                    else if (sequence[r] == '*')
                    {
                        result[w++] = Sequences.ZeroOrMany;
                    }
                    else
                    {
                        result[w++] = UnicodeMap.FromCharToLink(sequence[r]);
                    }
                }
            }
            else
            {
                for (var r = 0; r < sequence.Length; r++)
                {
                    if (IsEscape(sequence[r])) // Last Escape Symbol Ignored
                    {
                        if ((r + 1) < sequence.Length)
                        {
                            result[w++] = UnicodeMap.FromCharToLink(sequence[r + 1]);
                            r++;
                        }
                    }
                    else
                    {
                        result[w++] = UnicodeMap.FromCharToLink(sequence[r]);
                    }
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEscape(char c)
        {
            return c == '\\' || c == '~';
        }

        private static void AppendLinkToString(StringBuilder sb, ulong link)
        {
            if (link <= (char.MaxValue + 1))
            {
                sb.Append(UnicodeMap.FromLinkToChar(link));
            }
            else
            {
                sb.Append($"({link})");
            }
        }
    }
}
