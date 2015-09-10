using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Platform.Links.System.Helpers.Disposal;

namespace Platform.Links.System.Helpers.Udp
{
    public delegate void MessageHandlerCallback(string message);

    /// <summary>
    /// Представляет вспомогательную сущность для получения сообщений по протоколу UDP.
    /// </summary>
    /// <remarks>
    /// TODO: Попробовать ThreadPool / Tasks
    /// </remarks>
    public class UdpReceiver : DisposalBase
    {
        private const int DefaultPort = 15000;

        private bool _stopReceive;
        private Thread _worker;
        private readonly UdpClient _udp;
        private readonly MessageHandlerCallback _messageHandler;

        public UdpReceiver(Thread worker, int listenPort, bool stopReceive, MessageHandlerCallback messageHandler)
        {
            _worker = worker;
            _udp = new UdpClient(listenPort);
            _stopReceive = stopReceive;
            _messageHandler = messageHandler;
        }

        public UdpReceiver(int listenPort, MessageHandlerCallback messageHandler)
            : this(null, listenPort, false, messageHandler)
        {
        }

        public UdpReceiver(MessageHandlerCallback messageHandler)
            : this(null, DefaultPort, false, messageHandler)
        {
        }

        public UdpReceiver()
            : this(null, DefaultPort, false, message => { })
        {
        }

        public void Start()
        {
            _stopReceive = false;
            _worker = new Thread(Receive);
            _worker.Start();
        }

        public void Stop()
        {
            _stopReceive = true;
            if (_udp != null) _udp.Close();
            if (_worker != null) _worker.Join();
        }

        // Функция извлекающая пришедшие сообщения
        // и работающая в отдельном потоке.
        private void Receive()
        {
            try
            {
                while (true)
                {
                    IPEndPoint ipendpoint = null;
                    byte[] message = _udp.Receive(ref ipendpoint);
                    _messageHandler(Encoding.Default.GetString(message));

                    // Если дана команда остановить поток, останавливаем бесконечный цикл.
                    if (_stopReceive) break;
                }
            }
            catch
            {
                // Log Exception
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