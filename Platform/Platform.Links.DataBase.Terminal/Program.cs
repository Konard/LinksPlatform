using System;
using Platform.Communication.Udp;

namespace Platform.Links.DataBase.Terminal
{
    internal static class Program
    {
        private static void Main()
        {
            using (var receiver = new UdpReceiver(8888, m => Console.WriteLine("R.M.: {0}", m)))
            {
                receiver.Start();

                using (var sender = new UdpSender(7777))
                {
                    Console.WriteLine("Welcome to sequences terminal.");

                    string line;
                    while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
                        sender.Send(line);

                    Console.WriteLine("Empty request. Press any key to terminate process.");
                }

                receiver.Stop();
            }

            Console.WriteLine();
            Console.ReadKey();
        }
    }
}