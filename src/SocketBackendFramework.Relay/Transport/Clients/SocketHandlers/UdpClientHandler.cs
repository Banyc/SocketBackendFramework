using System.Net;
using SocketBackendFramework.Relay.Models.Delegates;

namespace SocketBackendFramework.Relay.Transport.Clients.SocketHandlers
{
    public class UdpClientHandlerBuilder : IClientHandlerBuilder
    {
        public IClientHandler Build(string ipAddress, int port)
        {
            return new UdpClientHandler(ipAddress, port);
        }
    }

    public class UdpClientHandler : NetCoreServer.UdpClient, IClientHandler
    {
        public event ReceivedEventHandler Received;
        public event ConnectionEventHandler Disconnected;
        public event ConnectionEventHandler Connected;

        public UdpClientHandler(string address, int port) : base(address, port)
        {
            this.remoteEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
        }

        protected override void OnConnected()
        {
            // cache the endpoint info
            this.localEndPoint = base.Socket.LocalEndPoint;
            this.remoteEndPoint = base.Socket.RemoteEndPoint;

            this.Connected?.Invoke(
                this,
                "udp",
                this.localEndPoint,
                this.remoteEndPoint);
            base.ReceiveAsync();  // correspond to official sample
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            base.OnReceived(endpoint, buffer, offset, size);
            this.Received?.Invoke(
                this,
                "udp",
                this.localEndPoint,
                endpoint,
                buffer, offset, size);
            base.ReceiveAsync();
        }

        protected override void OnDisconnected()
        {
            this.Disconnected?.Invoke(
                this,
                "udp",
                this.localEndPoint,
                this.remoteEndPoint);
        }

        #region IClientHandler
        void IClientHandler.Connect()
        {
            this.Connect();
        }

        void IClientHandler.Send(byte[] buffer, long offset, long size)
        {
            this.Send(buffer, offset, size);
        }

        void IClientHandler.Disconnect()
        {
            this.Disconnect();
        }

        private EndPoint localEndPoint = null;
        EndPoint IClientHandler.LocalEndPoint { get => this.localEndPoint; }

        private EndPoint remoteEndPoint = null;
        EndPoint IClientHandler.RemoteEndPoint { get => this.remoteEndPoint; }

        string IClientHandler.TransportType { get => "udp"; }
        #endregion
    }
}
