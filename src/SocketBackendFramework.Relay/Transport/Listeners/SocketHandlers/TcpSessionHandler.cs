using System.Net;
using NetCoreServer;

namespace SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers
{
    public class TcpSessionHandler : TcpSession
    {
        public delegate void ReceivedEventHandler(object sender, EndPoint remoteEndpoint, byte[] buffer, long offset, long size);
        public event ReceivedEventHandler Received;

        public delegate void DisconnectedEventHandler(object sender);
        public event DisconnectedEventHandler Disconnected;

        public TcpSessionHandler(TcpServer server) : base(server)
        {
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            base.OnReceived(buffer, offset, size);
            EndPoint remoteEndpoint = this.Socket.RemoteEndPoint;
            this.Received?.Invoke(this, remoteEndpoint, buffer, offset, size);
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            this.Disconnected?.Invoke(this);
        }
    }
}
