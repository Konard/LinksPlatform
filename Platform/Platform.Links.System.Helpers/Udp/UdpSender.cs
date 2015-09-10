using System.Net;
using System.Net.Sockets;
using System.Text;
using Platform.Links.System.Helpers.Disposal;

namespace Platform.Links.System.Helpers.Udp
{
    /// <summary>
    /// Представляет вспомогательную сущность для отправки сообщений по протоколу UDP.
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
            return _udp.Send(Encoding.Default.GetBytes(message), message.Length, _ipendpoint);
        }

        protected override void DisposeCore(bool manual)
        {
            _udp.Close();
        }
    }
}
