using System.Net;
using static SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers.TcpSessionHandler;

namespace SocketBackendFramework.Relay.Transport.Clients.SocketHandlers
{
    public class TcpClientHandler : NetCoreServer.TcpClient
    {
        public delegate void TcpClientHandlerConnectedEventHandler(object sender);
        public event TcpClientHandlerConnectedEventHandler Connected;
        public event ReceivedEventHandler Received;
        public event DisconnectedEventHandler Disconnected;

        // null if connection has been established
        private Queue<byte[]>? pendingTransmission = new();

        public TcpClientHandler(string address, int port) : base(address, port)
        {
        }

        protected override void OnConnected()
        {
            this.Connected?.Invoke(this);
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
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            EndPoint remoteEndPoint = this.Socket.RemoteEndPoint;
            base.OnReceived(buffer, offset, size);
            this.Received?.Invoke(this, remoteEndPoint, buffer, offset, size);
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            this.Disconnected?.Invoke(this);
        }

        public void SendAfterConnected(byte[] buffer)
        {
            if (this.IsConnected)
            {
                // send directly if no pendingTransmission
                if (this.pendingTransmission == null)
                {
                    base.Send(buffer);
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
    }
}
