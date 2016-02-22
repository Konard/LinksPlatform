using System;
using System.Threading;
using Platform.Communication.Protocol.Udp;
using Platform.Data.Core.Pairs;

namespace Platform.Data.SlaveServer
{
    public class Program
    {
        private static bool LinksServerStoped;

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                LinksServerStoped = true;
            };

            var links = new Links2();

            //var sequences = new Sequences(links);

            Console.WriteLine("Links slave server started.");

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
                            //sequences.Search(sender, m);
                        }
                    }
                }))
                {
                    while (!LinksServerStoped) Thread.Sleep(1);
                }
            }

            Console.WriteLine("Links slave server stopped.");
        }
    }
}