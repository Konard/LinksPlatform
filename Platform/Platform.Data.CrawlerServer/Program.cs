using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using CsQuery.ExtensionMethods;
using log4net;
using log4net.Config;
using Platform.Communication.Protocol.Udp;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;
using Platform.Helpers.Threading;

namespace Platform.Data.CrawlerServer
{
    internal static class Program
    {
        private const string DefaultDatabaseFilename = "db.links";

        public static bool LinksServerRunning = true;
        private static UnicodeMap UnicodeMap;

        public static ConcurrentBag<Thread> Threads = new ConcurrentBag<Thread>();

        private static void Main()
        {
            XmlConfigurator.Configure();

            Console.CancelKeyPress += OnCancelKeyPressed;

            var logger = LogManager.GetLogger(Assembly.GetExecutingAssembly(), "AbotLogger");

            try
            {
                using (var memoryManager = new LinksMemoryManager(DefaultDatabaseFilename, 8 * 1024 * 1024))
                using (var links = new Links(memoryManager))
                {
                    UnicodeMap = new UnicodeMap(links);
                    UnicodeMap.Init();

                    ulong pageMarker;
                    if (links.Exists(UnicodeMap.MapSize + 1))
                        pageMarker = UnicodeMap.MapSize + 1;
                    else
                        pageMarker = links.Create(LinksConstants.Itself, LinksConstants.Itself);

                    ulong sequencesMarker;
                    if (links.Exists(UnicodeMap.MapSize + 2))
                        sequencesMarker = UnicodeMap.MapSize + 2;
                    else
                        sequencesMarker = links.Create(LinksConstants.Itself, LinksConstants.Itself);

                    var sequencesOptions = new SequencesOptions();
                    sequencesOptions.UseCompression = true;
                    sequencesOptions.UseSequenceMarker = true;
                    sequencesOptions.SequenceMarkerLink = sequencesMarker;

                    var sequences = new Sequences(links, sequencesOptions);

                    Console.WriteLine("Сервер запущен.");
                    Console.WriteLine("Нажмите CTRL+C или ESC чтобы остановить.");

                    using (var sender = new UdpSender(8888))
                    {
                        MessageHandlerCallback handleMessage = message =>
                        {
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                message = message.Trim();

                                Console.WriteLine("<- {0}", message);

                                Uri siteUri;
                                if (Uri.TryCreate(message, UriKind.Absolute, out siteUri))
                                {
                                    Threads.Add(ThreadHelpers.StartNew(() => new Crawler(links, sequences, pageMarker).Start(siteUri)));

                                    sender.Send(string.Format("Сайт {0} добавлен в очередь на обработку.", siteUri));

                                    return;
                                }

                                //if (IsSearch(message))
                                sequences.Search(links, pageMarker, sender, ProcessSequenceForSearch(message + "?"));
                                //else
                                //    sequences.Create(sender, ProcessSequenceForCreate(message));
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

                            Console.WriteLine("Ожидаем завершения процессов...");

                            Threads.ToArray().ForEach(x => x.Join());

                            Console.WriteLine("Сервер остановлен.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }

            Console.CancelKeyPress -= OnCancelKeyPressed;
        }

        private static void OnCancelKeyPressed(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            LinksServerRunning = false;
        }

        private static void AppendLinkToString(StringBuilder sb, ulong link)
        {
            if (link <= (char.MaxValue + 1))
                sb.Append(UnicodeMap.FromLinkToChar(link));
            else
                sb.AppendFormat("({0})", link);
        }

        private static bool IsSearch(string message)
        {
            var i = message.Length - 1;
            if (message[i] == '?')
            {
                var escape = 0;
                while (--i >= 0 && IsEscape(message[i]))
                    escape++;
                return escape % 2 == 0;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEscape(char c)
        {
            return c == '\\' || c == '~';
        }

        private static ulong[] ProcessSequenceForSearch(string sequence)
        {
            return ProcessSequence(sequence, forSearch: true);
        }

        private static ulong[] ProcessSequenceForCreate(string sequence)
        {
            return ProcessSequence(sequence, forSearch: false);
        }

        private static ulong[] ProcessSequence(string sequence, bool forSearch = false)
        {
            var sequenceLength = sequence.Length;
            if (forSearch) sequenceLength--;
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
                    w++;
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
                        result[w++] = LinksConstants.Any;
                    else if (sequence[r] == '*')
                        result[w++] = Sequences.ZeroOrMany;
                    else
                        result[w++] = UnicodeMap.FromCharToLink(sequence[r]);
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
                        result[w++] = UnicodeMap.FromCharToLink(sequence[r]);
                }
            }

            return result;
        }

        private static void Create(this Sequences sequences, UdpSender sender, ulong[] sequence)
        {
            var link = sequences.Create(sequence);

            sender.Send(string.Format("Sequence with balanced variant at {0} created.", link));
        }

        private static void Search(this Sequences sequences, Links links, ulong pageMarker, UdpSender sender, ulong[] sequence)
        {
            var containsAny = Array.IndexOf(sequence, LinksConstants.Any) >= 0;
            var containsZeroOrMany = Array.IndexOf(sequence, Sequences.ZeroOrMany) >= 0;

            //if (containsZeroOrMany)
            //{
            //    var patternMatched = sequences.MatchPattern(sequence);

            //    sender.Send(string.Format("{0} sequences matched pattern.", patternMatched.Count));
            //    foreach (var result in patternMatched)
            //        sender.Send(string.Format("\t{0}: {1}", result, sequences.FormatSequence(result, AppendLinkToString, false)));
            //}

            if (!containsAny && !containsZeroOrMany)
            {
                //var sw = Stopwatch.StartNew();

                //int counter = 0;

                //sequences.Each(link =>
                //{
                //    sender.Send(string.Format("\t{0}: {1}", link,
                //        sequences.FormatSequence(link, AppendLinkToString, false)));

                //    counter++;

                //    return counter < 10;
                //}, sequence);

                //sw.Stop();

                //sender.Send(string.Format("Точных соответствий: {0}. За {1} мс.", counter, sw.ElapsedMilliseconds));


                var sw = Stopwatch.StartNew();

                var counter = 0;

                sequences.GetAllPartiallyMatchingSequences2(link =>
                {
                    if (links.GetSourceCore(link) == pageMarker)
                    {
                        var target = links.GetTargetCore(link);
                        var targetSource = links.GetSourceCore(target);

                        //links.Each(pageMarker, 0, page =>
                        //{
                        //    return true;
                        //});

                        sender.Send(string.Format("\t{0}: {1}", link,
                            sequences.FormatSequence(targetSource, AppendLinkToString, false)));
                        counter++;
                    }

                    return counter < 10;
                }, sequence);

                sw.Stop();

                sender.Send(string.Format("Частичных соответствий: {0}. За {1} мс.", counter, sw.ElapsedMilliseconds));
            }
            else
            {
                sender.Send("Поиск по шаблону временно не поддерживается.");
            }

            //if (!containsAny && !containsZeroOrMany)
            //{
            //    var partiallyMatched = sequences.GetAllPartiallyMatchingSequences1(sequence);

            //    sender.Send(string.Format("{0} sequences matched partially.", partiallyMatched.Count));
            //    foreach (var result in partiallyMatched)
            //        sender.Send(string.Format("\t{0}: {1}", result,
            //            sequences.FormatSequence(result, AppendLinkToString, false)));

            //    var allConnections = sequences.GetAllConnections(sequence);

            //    sender.Send(string.Format("{0} sequences connects query elements.", allConnections.Count));
            //    foreach (var result in allConnections)
            //        sender.Send(string.Format("\t{0}: {1}", result,
            //            sequences.FormatSequence(result, AppendLinkToString, false)));
            //}
        }
    }
}
