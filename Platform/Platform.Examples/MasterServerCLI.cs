using System;
using System.IO;
using System.Net.Sockets;
using Platform.Exceptions;
using Platform.Threading;
using Platform.IO;
using Platform.Communication.Protocol.Udp;
using Platform.Data.Doublets;
using Platform.Data.Doublets.ResizableDirectMemory.Specific;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    public class MasterServerCLI : ICommandLineInterface
    {
        private const string DefaultDatabaseFilename = "db.links";

        public void Run(params string[] args)
        {
            try
            {
#if DEBUG
                File.Delete(DefaultDatabaseFilename);
#endif
                using (var cancellation = new ConsoleCancellation())
                using (var memoryAdapter = new UInt64ResizableDirectMemoryLinks(DefaultDatabaseFilename, 8 * 1024 * 1024))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    var unicodeMap = new UnicodeMap(syncLinks);
                    unicodeMap.Init();
                    var sequences = new Sequences(syncLinks, new SequencesOptions<ulong> { UseSequenceMarker = true, SequenceMarkerLink = 65537, UseCompression = true });
                    Console.WriteLine("Links server started.");
                    Console.WriteLine("Press CTRL+C or ESC to stop server.");
                    using (var sender = new UdpSender(8888))
                    {
                        var masterServer = new MasterServer(links, sequences, sender);
                        masterServer.PrintContents(Console.WriteLine);
                        void handleMessage(string message)
                        {
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                message = message.Trim();
                                Console.WriteLine($"<- {message}");
                                if (masterServer.IsSearch(message))
                                {
                                    masterServer.Search(message);
                                }
                                else
                                {
                                    masterServer.Create(message);
                                }
                            }
                        }
                        //using (var receiver = new UdpReceiver(7777, handleMessage))
                        using (var receiver = new UdpClient(7777))
                        {
                            while (cancellation.NotRequested)
                            {
                                while (receiver.Available > 0)
                                {
                                    handleMessage(receiver.ReceiveString());
                                }
                                while (Console.KeyAvailable)
                                {
                                    var info = Console.ReadKey(true);
                                    if (info.Key == ConsoleKey.Escape)
                                    {
                                        cancellation.ForceCancellation();
                                    }
                                }
                                ThreadHelpers.Sleep();
                            }
                            Console.WriteLine("Links server stopped.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToStringWithAllInnerExceptions());
            }
        }
    }
}
