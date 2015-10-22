using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Platform.Communication.Udp;
using Platform.Data.Core.Sequences;

namespace Platform.Data.MasterServer
{
    internal static class Program
    {
        private const string DefaultDatabaseFilename = "db.links";

        private static bool UTF16Initialized;
        private static ulong UTF16FirstCharLink;
        private static ulong UTF16LastCharLink;
        private static bool LinksServerStoped;

        private static void Main()
        {
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                LinksServerStoped = true;
            };

            File.Delete(DefaultDatabaseFilename);

            using (var links = new CoreUnsafe.Pairs.Links(DefaultDatabaseFilename, 512 * 1024 * 1024))
            {
                InitUTF16(links);

                PrintContents(links);

                var sequences = new Sequences(links);

                Console.WriteLine("Links server started.");
                Console.WriteLine("Press CTRL+C to stop server.");

                using (var sender = new UdpSender(8888))
                {
                    using (var receiver = new UdpReceiver(7777, m =>
                    {
                        Console.WriteLine("R.M.: {0}", m);

                        if (!string.IsNullOrWhiteSpace(m))
                        {
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
                        receiver.Start();

                        while (!LinksServerStoped) Thread.Sleep(1);

                        receiver.Stop();
                    }
                }

                Console.WriteLine("Links server stopped.");
            }
        }

        private static void PrintContents(CoreUnsafe.Pairs.Links links)
        {
            if (links.Total == char.MaxValue)
                Console.WriteLine("Database is empty.");
            else
            {
                Console.WriteLine("Contents:");

                int linksTotalLength = links.Total.ToString("0").Length;

                var printFormatBase = new String('0', linksTotalLength);

                // Выделить код по печати одной связи в Extensions

                string printFormat = string.Format("\t[{{0:{0}}}]: {{1:{0}}} -> {{2:{0}}} ({{3}})", printFormatBase);

                for (ulong link = UTF16LastCharLink + 1; link <= links.Total; link++)
                {
                    Console.WriteLine(printFormat, link, links.GetSource(link), links.GetTarget(link),
                        links.GetLink(link));
                }
            }
        }

        private static void InitUTF16(CoreUnsafe.Pairs.Links links)
        {
            if (UTF16Initialized)
                return;

            UTF16Initialized = true;
            UTF16FirstCharLink = 1;
            UTF16LastCharLink = UTF16FirstCharLink + char.MaxValue - 1;

            ulong firstLink = links.Create(0, 0);

            if (firstLink != UTF16FirstCharLink)
            {
                links.Delete(firstLink);
                Console.WriteLine("Assume UTF16 table already created.");
            }
            else
            {
                for (ulong i = UTF16FirstCharLink + 1; i <= UTF16LastCharLink; i++)
                {
                    ulong createdLink = links.Create(0, 0);
                    if (createdLink != i)
                        throw new Exception("Unable to initialize UTF 16 table.");
                }

                Console.WriteLine("UTF16 table created and initialized.");
            }

            Console.WriteLine("Total links count: {0}.", links.Total);
        }

        private static void Create(this Sequences sequences, UdpSender sender, string sequence)
        {
            // char array to ulong array
            var linksSequence = new ulong[sequence.Length];
            for (int i = 0; i < sequence.Length; i++)
                linksSequence[i] = sequence[i];

            ulong resultLink = sequences.Create(linksSequence);

            sender.Send(string.Format("Sequence with balanced variant at {0} created.", resultLink));
        }

        private static string FromLinkToString(ulong link)
        {
            if (char.MaxValue >= link)
                return ((char) link).ToString(CultureInfo.InvariantCulture);
            else
                return link.ToString(CultureInfo.InvariantCulture);
        }

        private static void Search(this Sequences sequences, UdpSender sender, string sequenceQuery)
        {
            var linksSequenceQuery = new ulong[sequenceQuery.Length];
            for (int i = 0; i < sequenceQuery.Length; i++)
                if (sequenceQuery[i] == '_') // Добавить экранирование \_ в качестве _ (или что-то в этом роде)
                    linksSequenceQuery[i] = Sequences.Any;
                else if(sequenceQuery[i] == '*')
                    linksSequenceQuery[i] = Sequences.ZeroOrMany;
                else
                    linksSequenceQuery[i] = sequenceQuery[i];

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