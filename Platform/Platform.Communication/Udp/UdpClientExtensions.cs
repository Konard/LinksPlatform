using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Platform.Communication.Udp
{
    public static class UdpClientExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendString(this UdpClient udp, IPEndPoint ipEndPoint, string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            return udp.Send(bytes, bytes.Length, ipEndPoint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReceiveString(this UdpClient udp)
        {
            IPEndPoint ipEndPoint = null;
            var bytes = udp.Receive(ref ipEndPoint);
            var message = Encoding.Default.GetString(bytes);
            return message;
        }
    }
}
