using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Platform.Communication.Protocol.Udp;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;
using Platform.Helpers;

namespace Platform.Examples
{
    public class MasterServerCLI : ICommandLineInterface
    {
        private const string DefaultDatabaseFilename = "db.links";

        private bool _linksServerRunning = true;

        public void Run(params string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPressed;

            try
            {
#if DEBUG
                File.Delete(DefaultDatabaseFilename);
#endif

                using (var memoryManager = new UInt64LinksMemoryManager(DefaultDatabaseFilename, 8 * 1024 * 1024))
                using (var links = new UInt64Links(memoryManager))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    var unicodeMap = new UnicodeMap(syncLinks);
                    unicodeMap.Init();

                    var sequences = new Sequences(syncLinks, new SequencesOptions { UseSequenceMarker = true, SequenceMarkerLink = 65537, UseCompression = true });

                    Console.WriteLine("Links server started.");
                    Console.WriteLine("Press CTRL+C or ESC to stop server.");

                    using (var sender = new UdpSender(8888))
                    {
                        var masterServer = new MasterServer(links, sequences, sender);

                        masterServer.PrintContents(Console.WriteLine);

                        MessageHandlerCallback handleMessage = message =>
                        {
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                message = message.Trim();

                                Console.WriteLine($"<- {message}");

                                if (masterServer.IsSearch(message))
                                    masterServer.Search(message);
                                else
                                    masterServer.Create(message);
                            }
                        };

                        //using (var receiver = new UdpReceiver(7777, handleMessage))
                        using (var receiver = new UdpClient(7777))
                        {
                            while (_linksServerRunning)
                            {
                                while (receiver.Available > 0)
                                    handleMessage(receiver.ReceiveString());

                                while (Console.KeyAvailable)
                                {
                                    var info = Console.ReadKey(true);
                                    if (info.Key == ConsoleKey.Escape)
                                        _linksServerRunning = false;
                                }

                                Thread.Sleep(1);
                            }

                            Console.WriteLine("Links server stopped.");
                        }
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
            _linksServerRunning = false;
        }
    }
}
