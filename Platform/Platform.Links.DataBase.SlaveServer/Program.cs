using System;
using System.Threading;
using Platform.Communication.Udp;

namespace Platform.Links.DataBase.SlaveServer
{
    internal class Program
    {
        private static bool LinksServerStoped;

        private static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                LinksServerStoped = true;
            };

            var links = new CoreNet.Pairs.Links();

            //var sequences = new Sequences(links);

            Console.WriteLine("Links slave server started.");

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
                            //sequences.Search(sender, m);
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
    }
}