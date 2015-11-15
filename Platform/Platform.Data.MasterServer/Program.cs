using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Platform.Communication.Udp;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;
using Platform.Helpers;

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
            Console.CancelKeyPress += OnCancelKeyPressed;

            try
            {
#if DEBUG
                File.Delete(DefaultDatabaseFilename);
#endif

                using (var memoryManager = new LinksMemoryManager(DefaultDatabaseFilename, 8 * 1024 * 1024))
                using (var links = new Links(memoryManager))
                {
                    InitUTF16(links);

                    var sequences = new Sequences(links);

                    PrintContents(links, sequences);

                    Console.WriteLine("Links server started.");
                    Console.WriteLine("Press CTRL+C or ESC to stop server.");


                    using (var sender = new UdpSender(8888))
                    {
                        MessageHandlerCallback handleMessage = message =>
                        {
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                Console.WriteLine("R.M.: {0}", message);

                                if (message.EndsWith("?"))
                                    sequences.Search(sender, message);
                                else
                                    sequences.Create(sender, message);
                            }
                        };

                        //using (var receiver = new UdpReceiver(7777, handleMessage))
                        using (var receiver = new UdpClient(7777))
                        {
                            while (LinksServerRunning)
                            {
                                while (receiver.Available > 0)
                                    handleMessage(receiver.ReceiveString());

                                while (Console.KeyAvailable)
                                {
                                    var info = Console.ReadKey(true);
                                    if (info.Key == ConsoleKey.Escape)
                                        LinksServerRunning = false;
                                }

                                Thread.Sleep(1);
                            }

                            Console.WriteLine("Links server stopped.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.WriteToConsole();
            }

            Console.CancelKeyPress -= OnCancelKeyPressed;
        }

        private static void OnCancelKeyPressed(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            LinksServerRunning = false;
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
                        sequences.FormatSequence(link, AppendLinkToString, true));
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

        private static void AppendLinkToString(StringBuilder sb, ulong link)
        {
            if (link <= (char.MaxValue + 1))
                sb.Append(FromLinkToChar(link));
            else
                sb.AppendFormat("({0})", link);
        }

        private static void Search(this Sequences sequences, UdpSender sender, string sequenceQuery)
        {
            var actualLength = sequenceQuery.Length - 1;
            var linksSequenceQuery = new ulong[actualLength];
            for (var i = 0; i < actualLength; i++)
                if (sequenceQuery[i] == '_') // Добавить экранирование \_ в качестве _ (или что-то в этом роде)
                    linksSequenceQuery[i] = LinksConstants.Any;
                else if (sequenceQuery[i] == '*')
                    linksSequenceQuery[i] = Sequences.ZeroOrMany;
                else
                    linksSequenceQuery[i] = FromCharToLink(sequenceQuery[i]);

            if (linksSequenceQuery.Contains(LinksConstants.Any) || linksSequenceQuery.Contains(Sequences.ZeroOrMany))
            {
                var patternMatched = sequences.MatchPattern(linksSequenceQuery);

                sender.Send(string.Format("{0} sequences matched pattern.", patternMatched.Count));
                foreach (var result in patternMatched)
                    sender.Send(string.Format("\t{0}: {1}", result, sequences.FormatSequence(result, AppendLinkToString, false)));
            }
            else
            {
                var fullyMatched = sequences.GetAllMatchingSequences1(linksSequenceQuery);

                sender.Send(string.Format("{0} sequences matched fully.", fullyMatched.Count));
                foreach (var result in fullyMatched)
                    sender.Send(string.Format("\t{0}: {1}", result, sequences.FormatSequence(result, AppendLinkToString, false)));

                var partiallyMatched = sequences.GetAllPartiallyMatchingSequences1(linksSequenceQuery);

                sender.Send(string.Format("{0} sequences matched partially.", partiallyMatched.Count));
                foreach (var result in partiallyMatched)
                    sender.Send(string.Format("\t{0}: {1}", result, sequences.FormatSequence(result, AppendLinkToString, false)));

                var allConnections = sequences.GetAllConnections(linksSequenceQuery);

                sender.Send(string.Format("{0} sequences connects query elements.", allConnections.Count));
                foreach (var result in allConnections)
                    sender.Send(string.Format("\t{0}: {1}", result, sequences.FormatSequence(result, AppendLinkToString, false)));
            }
        }
    }
}