using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        private bool _stopReceive;
        private Thread _worker;
        private readonly int _listenPort;
        private readonly UdpClient _udp;
        private readonly MessageHandlerCallback _messageHandler;

        public UdpReceiver(Thread worker, int listenPort, bool autoStart, MessageHandlerCallback messageHandler)
        {
            _worker = worker;
            _listenPort = listenPort;
            _udp = new UdpClient(listenPort);
            _stopReceive = true;
            _messageHandler = messageHandler;

            if (autoStart) Start();
        }

        public UdpReceiver(int listenPort, MessageHandlerCallback messageHandler)
            : this(null, listenPort, true, messageHandler)
        {
        }

        public UdpReceiver(MessageHandlerCallback messageHandler)
            : this(null, DefaultPort, true, messageHandler)
        {
        }

        public UdpReceiver()
            : this(null, DefaultPort, true, message => { })
        {
        }

        public void Start()
        {
            if (_stopReceive)
            {
                _stopReceive = false;
                _worker = new Thread(Receive);
                _worker.Start();
            }
        }

        public void Stop()
        {
            if (!_stopReceive)
            {
                _stopReceive = true;

                // Send Packet to itself to switch Receiver from Receiving.
                _udp.Connect(IPAddress.Loopback, _listenPort);
                _udp.Send(new byte[0], 0);

                if (_worker != null) _worker.Join();
                if (_udp != null) _udp.Close();
            }
        }

        // Функция извлекающая пришедшие сообщения
        // и работающая в отдельном потоке.
        private void Receive()
        {
            while (true)
            {
                try
                {
                    IPEndPoint ipendpoint = null;
                    byte[] message = _udp.Receive(ref ipendpoint);
                    _messageHandler(Encoding.Default.GetString(message));
                }
                catch (Exception ex)
                {
                    // TODO: Log Exception
                }

                // Если дана команда остановить поток, останавливаем бесконечный цикл.
                if (_stopReceive) break;
            }
        }

        protected override void DisposeCore(bool manual)
        {
            try
            {
                Stop();
            }
            finally
            {
                _worker = null;
            }
        }
    }
}