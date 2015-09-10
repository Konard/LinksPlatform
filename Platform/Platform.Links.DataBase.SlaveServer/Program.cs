using System;
using System.Threading;
using Platform.Links.System.Helpers.Udp;

namespace Platform.Links.DataBase.SlaveServer
{
    class Program
    {
        //static void Main(string[] args)
        //{
        //    Console.WriteLine("Links Slave.");
        //    Console.ReadKey();
        //}

        private static bool UTF16Initialized = false;
        static ulong UTF16FirstCharLink;
        private static ulong UTF16LastCharLink;
        private static bool LinksServerStoped = false;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                LinksServerStoped = true;
            };

            var links = new CoreNet.Links();

            //InitUTF16(links);

            PrintContents(links);

            //var sequences = new Sequences(links);

            Console.WriteLine("Links slave server started.");

            using (var sender = new UdpSender(8888))
            {
                using (var receiver = new UdpReceiver(7777, (m) =>
                {
                    Console.WriteLine("R.M.: {0}", m);

                    if (!string.IsNullOrWhiteSpace(m))
                    {
                        if (m.EndsWith("?"))
                        {
                            m = m.Remove(m.Length - 1);
                            //sequences.Search(sender, m);
                        }
                        else
                        {
                            //sequences.Create(sender, m);
                        }
                    }

                }))
                {
                    receiver.Start();

                    while (!LinksServerStoped) Thread.Sleep(1);

                    receiver.Stop();
                }
            }

            Console.WriteLine("Links slave server stopped.");

        }

        // Устранить дублирование
        private static void PrintContents(CoreNet.Links links)
        {
            if (links.Total == char.MaxValue)
                Console.WriteLine("Database is empty.");
            else
            {
                Console.WriteLine("Contents:");

                var linksTotalLength = links.Total.ToString("0").Length;

                var printFormatBase = new String('0', linksTotalLength);

                var printFormat = string.Format("\t[{{0:{0}}}]: {{1:{0}}} -> {{2:{0}}} ({{3}})", printFormatBase);

                for (var link = UTF16LastCharLink + 1; link <= links.Total; link++)
                {
                    //Console.WriteLine(printFormat, link, links.GetSource(link), links.GetTarget(link), links.GetLink(link));
                }
            }
        }

        //private static void InitUTF16(Net.Links links)
        //{
        //    if (UTF16Initialized)
        //        return;

        //    UTF16Initialized = true;
        //    UTF16FirstCharLink = 1;
        //    UTF16LastCharLink = UTF16FirstCharLink + char.MaxValue - 1;

        //    var firstLink = links.Create(0, 0);

        //    if (firstLink != UTF16FirstCharLink)
        //    {
        //        links.Delete(firstLink);
        //        Console.WriteLine("Assume UTF16 table already created.");
        //    }
        //    else
        //    {
        //        for (var i = UTF16FirstCharLink + 1; i <= UTF16LastCharLink; i++)
        //        {
        //            var createdLink = links.Create(0, 0);
        //            if (createdLink != i)
        //                throw new Exception("Unable to initialize UTF 16 table.");
        //        }

        //        Console.WriteLine("UTF16 table created and initialized.");
        //    }

        //    Console.WriteLine("Total links count: {0}.", links.Total);
        //}

        //private static void Create(this Sequences sequences, UdpSender sender, string sequence)
        //{
        //    var linksSequence = new ulong[sequence.Length];
        //    for (var i = 0; i < sequence.Length; i++)
        //        linksSequence[i] = sequence[i];

        //    var resultLink = sequences.Create(linksSequence);

        //    sender.Send(string.Format("Sequence with balanced variant at {0} created.", resultLink));
        //}

        //private static void Search(this Sequences sequences, UdpSender sender, string sequenceQuery)
        //{
        //    var linksSequenceQuery = new ulong[sequenceQuery.Length];
        //    for (var i = 0; i < sequenceQuery.Length; i++)
        //        if (sequenceQuery[i] == '_')
        //            linksSequenceQuery[i] = 0;
        //        else
        //            linksSequenceQuery[i] = sequenceQuery[i];

        //    var resultList = sequences.Each(linksSequenceQuery);

        //    if (resultList.Count == 0)
        //        sender.Send("No sequences found.");
        //    else if (resultList.Count == 1)
        //        sender.Send(string.Format("Sequence found - {0}.", resultList[0]));
        //    else
        //    {
        //        sender.Send(string.Format("Found {0} sequences:", resultList.Count));

        //        for (var i = 0; i < resultList.Count; i++)
        //            sender.Send(string.Format("\t{0}", resultList[i]));
        //    }
        //}
    }
}
