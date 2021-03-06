using System.Net;
using SocketBackendFramework.Relay.Models.Delegates;

namespace SocketBackendFramework.Relay.Transport.Clients.SocketHandlers
{
    public class UdpClientHandlerBuilder : IClientHandlerBuilder
    {
        public IClientHandler Build(IPEndPoint remoteEndPoint, object? config)
        {
            return new UdpClientHandler(remoteEndPoint);
        }
    }

    public class UdpClientHandler : NetCoreServer.UdpClient, IClientHandler
    {
        public event ReceivedEventHandler? Received;
        public event ConnectionEventHandler? Disconnected;
        public event ConnectionEventHandler? Connected;

        public string TransportType => "udp";

        public UdpClientHandler(IPEndPoint remoteEndPoint) : base(remoteEndPoint)
        {
            this.remoteEndPoint = remoteEndPoint;
        }

        protected override void OnConnected()
        {
            // cache the endpoint info
            this.localEndPoint = base.Socket!.LocalEndPoint!;
            // the UDP Socket is not connected
            // this is null
            // this.remoteEndPoint = base.Socket!.RemoteEndPoint!;

            this.Connected?.Invoke(
                this,
                this.TransportType,
                this.localEndPoint,
                this.remoteEndPoint!);
            base.ReceiveAsync();  // correspond to official sample
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            base.OnReceived(endpoint, buffer, offset, size);
            this.Received?.Invoke(
                this,
                this.TransportType,
                this.localEndPoint!,
                endpoint,
                buffer, offset, size);
            base.ReceiveAsync();
        }

        protected override void OnDisconnected()
        {
            this.Disconnected?.Invoke(
                this,
                this.TransportType,
                this.localEndPoint!,
                this.remoteEndPoint!);
        }

        #region IClientHandler
        void IClientHandler.Connect()
        {
            this.Connect();
        }

        void IClientHandler.Send(byte[] buffer, long offset, long size)
        {
            this.SendAsync(buffer, offset, size);
        }

        void IClientHandler.Disconnect()
        {
            this.Disconnect();
        }

        private EndPoint? localEndPoint = null;
        EndPoint? IClientHandler.LocalEndPoint { get => this.localEndPoint; }

        private readonly EndPoint? remoteEndPoint = null;
        EndPoint? IClientHandler.RemoteEndPoint { get => this.remoteEndPoint; }
        #endregion
    }
}
