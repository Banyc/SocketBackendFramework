using System.Net;
using static SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers.UdpServerHandler;

namespace SocketBackendFramework.Relay.Transport.Clients.SocketHandlers
{
    public class UdpClientHandler : NetCoreServer.UdpClient
    {
        public event ReceivedEventHandler Received;

        public UdpClientHandler(string address, int port) : base(address, port)
        {
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
