using System;
using System.Net.Sockets;
using System.Threading;
using Platform.Communication.Udp;
using Platform.Helpers;

namespace Platform.Data.ConsoleTerminal
{
    internal static class Program
    {
        private static bool TerminalRunning = true;

        private static void Main()
        {
            Console.CancelKeyPress += OnCancelKeyPressed;

            try
            {
                using (var receiver = new UdpClient(8888))
                {
                    using (var sender = new UdpSender(7777))
                    {
                        Console.WriteLine("Welcome to sequences terminal.");
                        Console.WriteLine("Press CTRL+C or enter empty line to stop terminal.");

                        while (TerminalRunning)
                        {
                            while (Console.KeyAvailable)
                            {
                                var line = Console.ReadLine();
                                if (!string.IsNullOrWhiteSpace(line))
                                    sender.Send(line);
                                else
                                    TerminalRunning = false;
                            }

                            while (receiver.Available > 0)
                            {
                                var message = receiver.ReceiveString();
                                if (!string.IsNullOrWhiteSpace(message))
                                    Console.WriteLine("← {0}", message);
                            }

                            Thread.Sleep(1);
                        }

                        Console.WriteLine("Terminal stopped.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToRecursiveString());
            }

            Console.CancelKeyPress -= OnCancelKeyPressed;
        }

        private static void OnCancelKeyPressed(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            TerminalRunning = false;
        }
    }
}