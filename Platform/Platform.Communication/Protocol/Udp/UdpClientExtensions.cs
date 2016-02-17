using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Helpers.Threading;

namespace Platform.Communication.Protocol.Udp
{
    public static class UdpClientExtensions
    {
        private static readonly Encoding DefaultEncoding = Encoding.GetEncoding(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendString(this UdpClient udp, IPEndPoint ipEndPoint, string message)
        {
            var bytes = DefaultEncoding.GetBytes(message);
            return udp.SendAsync(bytes, bytes.Length, ipEndPoint).AwaitResult();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReceiveString(this UdpClient udp)
        {
            var bytes = udp.ReceiveAsync().AwaitResult().Buffer;
            var message = DefaultEncoding.GetString(bytes);
            return message;
        }
    }
}
