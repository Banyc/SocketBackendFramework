using System.Collections.Generic;
using System.Net;
using SocketBackendFramework.Relay.Models.Delegates;

namespace SocketBackendFramework.Relay.Transport.Clients.SocketHandlers
{
    public class TcpClientHandlerBuilder : IClientHandlerBuilder
    {
        public IClientHandler Build(string ipAddress, int port, string? configId)
        {
            return new TcpClientHandler(ipAddress, port);
        }
    }

    public class TcpClientHandler : NetCoreServer.TcpClient, IClientHandler
    {
        public event ConnectionEventHandler? Connected;
        public event ReceivedEventHandler? Received;
        public event ConnectionEventHandler? Disconnected;

        // null if connection has been established
        private Queue<byte[]>? pendingTransmission = new();

        public TcpClientHandler(string address, int port) : base(address, port)
        {
            this.remoteEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
        }

        protected override void OnConnected()
        {
            // cache endpoint info
            this.localEndPoint = base.Socket!.LocalEndPoint!;
            this.remoteEndPoint = base.Socket!.RemoteEndPoint!;

            this.Connected?.Invoke(
                this,
                "tcp",
                this.localEndPoint,
                this.remoteEndPoint);
            lock (this.pendingTransmission!)
            {
                while (this.pendingTransmission.Count > 0)
                {
                    byte[] buffer = this.pendingTransmission.Dequeue();
                    base.Send(buffer);
                }
                // discard queue since the connection has been established
                this.pendingTransmission = null;
            }
            // base.ReceiveAsync();  // TcpClient will automatically start receiving during connection which is established by `ConnectAsync`.
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
            // base.ReceiveAsync();  // TcpClient will try to receive again after exiting base.OnReceived
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

        public void SendAfterConnected(byte[] buffer, long offset, long size)
        {
            if (this.IsConnected)
            {
                // send directly if no pendingTransmission
                if (this.pendingTransmission == null)
                {
                    base.Send(buffer, offset, size);
                }
                else
                {
                    // otherwise, enqueue this buffer to avoid queue jumping
                    lock (this.pendingTransmission)
                    {
                        this.pendingTransmission.Enqueue(buffer);
                    }
                }
            }
            else
            {
                lock (this.pendingTransmission!)
                {
                    this.pendingTransmission.Enqueue(buffer);
                }
            }
        }

        #region IClientHandler
        void IClientHandler.Connect()
        {
            this.ConnectAsync();
        }

        void IClientHandler.Send(byte[] buffer, long offset, long size)
        {
            this.SendAfterConnected(buffer, offset, size);
        }

        void IClientHandler.Disconnect()
        {
            this.DisconnectAsync();
        }

        private EndPoint? localEndPoint = null;
        EndPoint? IClientHandler.LocalEndPoint { get => this.localEndPoint; }

        private EndPoint? remoteEndPoint = null;
        EndPoint? IClientHandler.RemoteEndPoint { get => this.remoteEndPoint; }

        string IClientHandler.TransportType { get => "tcp"; }
        #endregion
    }
}
