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
        public event SimpleEventHandler Disconnected;
        public event SimpleEventHandler Connected;

        public UdpClientHandler(string address, int port) : base(address, port)
        {
        }

        protected override void OnConnected()
        {
            this.Connected?.Invoke(this);
            base.ReceiveAsync();  // correspond to official sample
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            base.OnReceived(endpoint, buffer, offset, size);
            this.Received?.Invoke(this, endpoint, buffer, offset, size);
            base.ReceiveAsync();
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            this.Disconnected?.Invoke(this);
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

        public EndPoint GetLocalEndPoint()
        {
            return base.Socket.LocalEndPoint;
        }
        #endregion
    }
}
