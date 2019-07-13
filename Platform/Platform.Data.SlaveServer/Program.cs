using System;
using Platform.Helpers.Console;
using Platform.Communication.Protocol.Udp;

namespace Platform.Data.SlaveServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var links = new Links2();

            //var sequences = new Sequences(links);

            Console.WriteLine("Links slave server started.");

            using (var cancellation = new ConsoleCancellationHandler())
            using (var sender = new UdpSender(8888))
            using (new UdpReceiver(7777, m =>
            {
                if (!string.IsNullOrWhiteSpace(m))
                {
                    Console.WriteLine($"R.M.: {m}");

                    if (m.EndsWith("?"))
                    {
                        m = m.Remove(m.Length - 1);
                    //sequences.Search(sender, m);
                }
                }
            }))
            {
                cancellation.Wait();
            }

            Console.WriteLine("Links slave server stopped.");
        }
    }
}