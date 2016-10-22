using System;
using System.Net.Sockets;
using System.Threading;
using Platform.Communication.Protocol.Udp;
using Platform.Helpers;

namespace Platform.Examples
{
    public class TerminalCLI : ICommandLineInterface
    {
        private bool _terminalRunning = true;

        public void Run(params string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPressed;

            try
            {
                using (var receiver = new UdpClient(8888))
                {
                    using (var sender = new UdpSender(7777))
                    {
                        Console.WriteLine("Welcome to terminal.");
                        Console.WriteLine("Press CTRL+C or enter empty line to stop terminal.");

                        while (_terminalRunning)
                        {
                            while (Console.KeyAvailable)
                            {
                                var line = Console.ReadLine();
                                if (!string.IsNullOrWhiteSpace(line))
                                    sender.Send(line);
                                else
                                    _terminalRunning = false;
                            }

                            while (receiver.Available > 0)
                            {
                                var message = receiver.ReceiveString();
                                if (!string.IsNullOrWhiteSpace(message))
                                    Console.WriteLine($"<- {message}");
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

        private void OnCancelKeyPressed(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _terminalRunning = false;
        }
    }
}
