using System;
using System.Net.Sockets;
using Platform.Exceptions;
using Platform.Threading;
using Platform.IO;
using Platform.Communication.Protocol.Udp;

namespace Platform.Examples
{
    public class TerminalCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            try
            {
                using (var cancellation = new ConsoleCancellationHandler(showDefaultIntroMessage: false))
                using (var receiver = new UdpClient(8888))
                using (var sender = new UdpSender(7777))
                {
                    Console.WriteLine("Welcome to terminal.");
                    Console.WriteLine("Press CTRL+C or enter empty line to stop terminal.");

                    while (cancellation.NoCancellationRequested)
                    {
                        while (Console.KeyAvailable)
                        {
                            var line = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(line))
                                sender.Send(line);
                            else
                                cancellation.ForceCancellation();
                        }

                        while (receiver.Available > 0)
                        {
                            var message = receiver.ReceiveString();
                            if (!string.IsNullOrWhiteSpace(message))
                                Console.WriteLine($"<- {message}");
                        }

                        ThreadHelpers.Sleep();
                    }

                    Console.WriteLine("Terminal stopped.");
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToRecursiveString());
            }
        }
    }
}
