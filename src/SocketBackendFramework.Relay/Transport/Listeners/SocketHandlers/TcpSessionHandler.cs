using System.Net;
using NetCoreServer;
using SocketBackendFramework.Relay.Models.Delegates;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;

namespace SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers
{
    public class TcpSessionHandler : TcpSession, IClientHandler
    {
        public event ReceivedEventHandler? Received;
        public event ConnectionEventHandler? Disconnected;
        public event ConnectionEventHandler? Connected;

        public TcpSessionHandler(TcpServerHandler server) : base(server)
        {
            this.localEndPoint = server.LocalEndPoint;
        }

        protected override void OnConnected()
        {
            // cache the endpoint info
            this.localEndPoint = base.Socket!.LocalEndPoint!;
            this.remoteEndPoint = base.Socket!.RemoteEndPoint!;

            this.Connected?.Invoke(
                this,
                "tcp",
                this.localEndPoint,
                this.remoteEndPoint);  // TcpServer has already acknowledged the connection
            // base.ReceiveAsync();  // TcpSession will automatically start receiving during connection.
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            base.OnReceived(buffer, offset, size);
            this.Received?.Invoke(
                this,
                "tcp",
                this.localEndPoint!,
                this.remoteEndPoint!,
                buffer, offset, size);
            // base.ReceiveAsync();  // TcpSession will try to receive again after exiting base.OnReceived
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            this.Disconnected?.Invoke(
                this,
                "tcp",
                this.localEndPoint!,
                this.remoteEndPoint!);
        }

        #region IClientHandler
        public string TransportType => "tcp";

        private EndPoint? localEndPoint = null;
        EndPoint? IClientHandler.LocalEndPoint => this.localEndPoint;

        private EndPoint? remoteEndPoint = null;
        EndPoint? IClientHandler.RemoteEndPoint => this.remoteEndPoint;
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
