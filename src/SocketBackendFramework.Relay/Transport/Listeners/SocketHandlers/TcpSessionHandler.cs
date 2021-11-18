using System.Net;
using NetCoreServer;
using SocketBackendFramework.Relay.Models.Delegates;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;

namespace SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers
{
    public class TcpSessionHandler : TcpSession, IClientHandler
    {
        public string TransportType => "tcp";

        public EndPoint LocalEndPoint => this.Socket.LocalEndPoint;

        public EndPoint RemoteEndPoint => this.Socket.RemoteEndPoint;

        public event ReceivedEventHandler Received;
        public event ConnectionEventHandler Disconnected;
        public event ConnectionEventHandler Connected;

        public TcpSessionHandler(TcpServer server) : base(server)
        {
        }

        protected override void OnConnected()
        {
            this.Connected?.Invoke(
                this,
                "tcp",
                this.LocalEndPoint,
                this.RemoteEndPoint);  // TcpServer has already acknowledged the connection
            // base.ReceiveAsync();  // TcpSession will automatically start receiving during connection.
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            base.OnReceived(buffer, offset, size);
            this.Received?.Invoke(
                this,
                "tcp",
                this.LocalEndPoint,
                this.RemoteEndPoint,
                buffer, offset, size);
            // base.ReceiveAsync();  // TcpSession will try to receive again after exiting base.OnReceived
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            this.Disconnected?.Invoke(
                this,
                "tcp",
                this.LocalEndPoint,
                this.RemoteEndPoint);
        }

        #region IClientHandler
        public void Connect()
        {
            throw new System.NotSupportedException();
        }

        void IClientHandler.Send(byte[] buffer, long offset, long size)
        {
            base.Send(buffer, offset, size);
        }

        void IClientHandler.Disconnect()
        {
            base.Disconnect();
        }
        #endregion
    }
}
