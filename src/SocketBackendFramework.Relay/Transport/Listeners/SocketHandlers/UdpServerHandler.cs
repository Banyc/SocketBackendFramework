using System.Net;
using NetCoreServer;
using SocketBackendFramework.Relay.Models.Delegates;

namespace SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers
{
    public class UdpServerHandlerBuilder : IServerHandlerBuilder
    {
        public IServerHandler Build(IPEndPoint localEndPoint, string? configId)
        {
            return new UdpServerHandler(localEndPoint);
        }
    }

    public class UdpServerHandler : UdpServer, IServerHandler
    {
        public string TransportType => "udp";

        public EndPoint LocalEndPoint { get; }

        public event ConnectionEventHandler? ClientConnected { add { } remove { } }
        public event ConnectionEventHandler? ClientDisconnected { add { } remove { } }
        public event ReceivedEventHandler? ClientMessageReceived;

        public UdpServerHandler(IPEndPoint localEndPoint) : base(localEndPoint)
        {
            this.LocalEndPoint = localEndPoint;
        }

        protected override void OnStarted()
        {
            base.OnStarted();
            base.ReceiveAsync();
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            this.ClientMessageReceived?.Invoke(
                this,
                "udp",
                this.LocalEndPoint,
                endpoint,
                buffer, offset, size);
            base.ReceiveAsync();
        }

        #region IServerHandler
        void IServerHandler.Start()
        {
            base.Start();
        }

        void IServerHandler.Send(EndPoint remoteEndPoint, byte[] buffer, long offset, long size)
        {
            base.SendAsync(remoteEndPoint, buffer, offset, size);
        }

        public void Disconnect(EndPoint remoteEndPoint)
        {
        }
        #endregion
    }
}
