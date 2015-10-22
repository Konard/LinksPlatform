using System.Net;
using System.Net.Sockets;
using System.Text;
using Platform.Helpers.Disposal;

namespace Platform.Communication.Udp
{
    /// <summary>
    /// Представляет отправителя сообщений по протоколу UDP.
    /// </summary>
    public class UdpSender : DisposalBase
    {
        private const string DefaultHostname = "127.0.0.1";

        private readonly UdpClient _udp;
        private readonly IPEndPoint _ipendpoint;

        public UdpSender(string hostname, int port)
        {
            _udp = new UdpClient();
            _ipendpoint = new IPEndPoint(IPAddress.Parse(hostname), port);
        }

        public UdpSender(int port)
            : this(DefaultHostname, port)
        {
        }

        public int Send(string message)
        {
            // Формирование оправляемого сообщения и его отправка.
            var bytes = Encoding.Default.GetBytes(message);
            return _udp.Send(bytes, bytes.Length, _ipendpoint);
        }

        protected override void DisposeCore(bool manual)
        {
            _udp.Close();
        }
    }
}