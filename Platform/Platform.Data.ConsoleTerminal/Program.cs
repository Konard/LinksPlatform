using System;
using Platform.Communication.Udp;

namespace Platform.Data.ConsoleTerminal
{
    internal static class Program
    {
        private static bool TerminalRunning = true;

        private static void Main()
        {
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                TerminalRunning = false;
            };

            using (new UdpReceiver(8888, m =>
            {
                if (!string.IsNullOrWhiteSpace(m)) Console.WriteLine("R.M.: {0}", m);
            }))
            {
                using (var sender = new UdpSender(7777))
                {
                    Console.WriteLine("Welcome to sequences terminal.");
                    Console.WriteLine("Press CTRL+C or enter empty line to stop terminal.");

                    while (TerminalRunning)
                    {
                        var line = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                            sender.Send(line);
                        else
                            TerminalRunning = false;
                    }
                }
            }
        }
    }
}