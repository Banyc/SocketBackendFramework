using System.Net;
using NetCoreServer;

namespace SocketBackendFramework.Reply.Listeners.SocketHandlers
{
    public class UdpServerHandler : UdpServer
    {
        public delegate void ReceivedEventHandler(object sender, EndPoint endpoint, byte[] buffer, long offset, long size);
        public event ReceivedEventHandler Received;

        public UdpServerHandler(IPAddress address, int port) : base(address, port)
        {
        }

        protected override void OnStarted()
        {
            base.OnStarted();
            ReceiveAsync();
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            base.OnReceived(endpoint, buffer, offset, size);
            this.Received?.Invoke(this, endpoint, buffer, offset, size);
            ReceiveAsync();
        }
    }
}
