using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Platform.Communication.Udp;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;

namespace Platform.Data.MasterServer
{
    internal static class Program
    {
        private const string DefaultDatabaseFilename = "db.links";

        private static bool UTF16Initialized;
        private static ulong UTF16FirstCharLink;
        private static ulong UTF16LastCharLink;
        private static bool LinksServerRunning = true;

        private static void Main()
        {
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                LinksServerRunning = false;
            };

#if DEBUG
            File.Delete(DefaultDatabaseFilename);
#endif

            using (var links = new Links(DefaultDatabaseFilename, 8 * 1024 * 1024))
            {
                InitUTF16(links);

                var sequences = new Sequences(links);

                PrintContents(links, sequences);

                Console.WriteLine("Links server started.");
                Console.WriteLine("Press CTRL+C to stop server.");

                using (var sender = new UdpSender(8888))
                {
                    using (new UdpReceiver(7777, m =>
                    {
                        if (!string.IsNullOrWhiteSpace(m))
                        {
                            Console.WriteLine("R.M.: {0}", m);

                            if (m.EndsWith("?"))
                            {
                                m = m.Remove(m.Length - 1);
                                sequences.Search(sender, m);
                            }
                            else
                                sequences.Create(sender, m);
                        }
                    }))
                    {
                        while (LinksServerRunning) Thread.Sleep(1);
                    }
                }

                Console.WriteLine("Links server stopped.");
            }
        }

        private static void PrintContents(Links links, Sequences sequences)
        {
            if (links.Total == UTF16LastCharLink)
                Console.WriteLine("Database is empty.");
            else
            {
                Console.WriteLine("Contents:");

                var linksTotalLength = links.Total.ToString("0").Length;

                var printFormatBase = new String('0', linksTotalLength);

                // Выделить код по печати одной связи в Extensions

                var printFormat = string.Format("\t[{{0:{0}}}]: {{1:{0}}} -> {{2:{0}}} {{3}}", printFormatBase);

                for (var link = UTF16LastCharLink + 1; link <= links.Total; link++)
                {
                    Console.WriteLine(printFormat, link, links.GetSource(link), links.GetTarget(link),
                        sequences.FormatSequence(link, FromLinkToString, true));
                }
            }
        }

        private static void InitUTF16(Links links)
        {
            if (UTF16Initialized)
                return;

            UTF16Initialized = true;
            UTF16FirstCharLink = 1;
            UTF16LastCharLink = UTF16FirstCharLink + char.MaxValue;

            var firstLink = links.Create(0, 0);

            if (firstLink != UTF16FirstCharLink)
            {
                links.Delete(firstLink);
                Console.WriteLine("Assume UTF16 table already created.");
            }
            else
            {
                for (var i = UTF16FirstCharLink + 1; i <= UTF16LastCharLink; i++)
                {
                    // From NIL to It (NIL -> Character) transformation meaning, (or infinite amount of NIL characters before actual Character)
                    var createdLink = links.Create(firstLink, 0);
                    if (createdLink != i)
                        throw new Exception("Unable to initialize UTF 16 table.");
                }

                Console.WriteLine("UTF16 table created and initialized.");
            }

            Console.WriteLine("Total links count: {0}.", links.Total);
        }

        private static void Create(this Sequences sequences, UdpSender sender, string sequence)
        {
            var link = sequences.Create(FromStringToLinkArray(sequence));

            sender.Send(string.Format("Sequence with balanced variant at {0} created.", link));
        }

        // 0 - null link
        // 1 - nil character (0 character)
        // ...
        // 65536 (0(1) + 65535 = 65536 possible values)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong FromCharToLink(char character)
        {
            return ((ulong)character + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char FromLinkToChar(ulong link)
        {
            return (char)(link - 1);
        }

        private static ulong[] FromStringToLinkArray(string sequence)
        {
            // char array to ulong array
            var linksSequence = new ulong[sequence.Length];
            for (var i = 0; i < sequence.Length; i++)
                linksSequence[i] = FromCharToLink(sequence[i]);
            return linksSequence;
        }

        private static string FromLinkToString(ulong link)
        {
            if (link <= (char.MaxValue + 1))
                return FromLinkToChar(link).ToString(CultureInfo.InvariantCulture);

            return string.Format("({0})", link);
        }

        private static void Search(this Sequences sequences, UdpSender sender, string sequenceQuery)
        {
            var linksSequenceQuery = new ulong[sequenceQuery.Length];
            for (var i = 0; i < sequenceQuery.Length; i++)
                if (sequenceQuery[i] == '_') // Добавить экранирование \_ в качестве _ (или что-то в этом роде)
                    linksSequenceQuery[i] = Sequences.Any;
                else if (sequenceQuery[i] == '*')
                    linksSequenceQuery[i] = Sequences.ZeroOrMany;
                else
                    linksSequenceQuery[i] = FromCharToLink(sequenceQuery[i]);

            if (linksSequenceQuery.Contains(Sequences.Any) || linksSequenceQuery.Contains(Sequences.ZeroOrMany))
            {
                var patternMatched = sequences.MatchPattern(linksSequenceQuery);

                sender.Send(string.Format("{0} sequences matched pattern.", patternMatched.Count));
                foreach (var result in patternMatched)
                    sender.Send(string.Format("\t{0}: {1}", result, sequences.FormatSequence(result, FromLinkToString, false)));
            }
            else
            {
                var fullyMatched = sequences.GetAllMatchingSequences1(linksSequenceQuery);

                sender.Send(string.Format("{0} sequences matched fully.", fullyMatched.Count));
                foreach (var result in fullyMatched)
                    sender.Send(string.Format("\t{0}: {1}", result, sequences.FormatSequence(result, FromLinkToString, false)));

                var partiallyMatched = sequences.GetAllPartiallyMatchingSequences1(linksSequenceQuery);

                sender.Send(string.Format("{0} sequences matched partially.", partiallyMatched.Count));
                foreach (var result in partiallyMatched)
                    sender.Send(string.Format("\t{0}: {1}", result, sequences.FormatSequence(result, FromLinkToString, false)));

                var allConnections = sequences.GetAllConnections(linksSequenceQuery);

                sender.Send(string.Format("{0} sequences connects query elements.", allConnections.Count));
                foreach (var result in allConnections)
                    sender.Send(string.Format("\t{0}: {1}", result, sequences.FormatSequence(result, FromLinkToString, false)));
            }
        }
    }
}