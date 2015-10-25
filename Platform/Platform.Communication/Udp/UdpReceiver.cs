using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Platform.Helpers.Disposal;

namespace Platform.Communication.Udp
{
    public delegate void MessageHandlerCallback(string message);

    /// <summary>
    /// Представляет получателя сообщений по протоколу UDP.
    /// </summary>
    /// <remarks>
    /// TODO: Попробовать ThreadPool / Tasks
    /// </remarks>
    public class UdpReceiver : DisposalBase
    {
        private const int DefaultPort = 15000;

        private bool _receiverRunning;
        private Thread _thread;
        private readonly int _listenPort;
        private readonly UdpClient _udp;
        private readonly MessageHandlerCallback _messageHandler;

        public bool Available { get { return _udp.Available > 0; } }

        public UdpReceiver(int listenPort, bool autoStart, MessageHandlerCallback messageHandler)
        {
            _udp = new UdpClient(listenPort);
            _listenPort = listenPort;
            _messageHandler = messageHandler;

            if (autoStart) Start();
        }

        public UdpReceiver(int listenPort, MessageHandlerCallback messageHandler)
            : this(listenPort, true, messageHandler)
        {
        }

        public UdpReceiver(MessageHandlerCallback messageHandler)
            : this(DefaultPort, true, messageHandler)
        {
        }

        public UdpReceiver()
            : this(DefaultPort, true, message => { })
        {
        }

        public void Start()
        {
            if (!_receiverRunning && _thread == null)
            {
                _receiverRunning = true;
                _thread = new Thread(Receiver);
                _thread.Start();
            }
        }

        public void Stop()
        {
            if (_receiverRunning && _thread != null)
            {
                _receiverRunning = false;

                // Send Packet to itself to switch Receiver from Receiving.
                _udp.Connect(IPAddress.Loopback, _listenPort);
                _udp.Send(new byte[0], 0);

                _thread.Join();
                _thread = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Receive()
        {
            return _udp.ReceiveString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReceiveAndHandle()
        {
            _messageHandler(Receive());
        }

        // Функция извлекающая пришедшие сообщения
        // и работающая в отдельном потоке.
        private void Receiver()
        {
            while (_receiverRunning)
            {
                try { ReceiveAndHandle(); }
                catch (Exception ex)
                {
                    // TODO: Log Exception
                }
            }
        }

        protected override void DisposeCore(bool manual)
        {
            Stop();
            _udp.Close();
        }
    }
}