using System.Net;
using NetCoreServer;
using SocketBackendFramework.Relay.Models.Delegates;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;

namespace SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers
{
    public class TcpSessionHandler : TcpSession
    {
        public IPEndPoint LocalIPEndPoint { get; private set; }
        public IPEndPoint RemoteIPEndPoint { get; private set; }

        public event ReceivedEventHandler Received;
        public event SimpleEventHandler Disconnected;
        public event SimpleEventHandler TcpSessionTimedOut;

        private readonly System.Timers.Timer timer;

        public TcpSessionHandler(TcpServer server, double timeoutMs) : base(server)
        {
            this.timer = new()
            {
                Interval = timeoutMs,
                AutoReset = false,
            };
            this.timer.Elapsed += (sender, e) => this.TcpSessionTimedOut?.Invoke(this);
            this.timer.Start();
        }

        public FiveTuples GetFiveTuples()
        {
            return new()
            {
                Local = this.LocalIPEndPoint,
                Remote = this.RemoteIPEndPoint,
                TransportType = ExclusiveTransportType.Tcp,
            };
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            this.LocalIPEndPoint = (IPEndPoint)base.Socket.LocalEndPoint;
            this.RemoteIPEndPoint = (IPEndPoint)base.Socket.RemoteEndPoint;
            // base.ReceiveAsync();  // TcpSession will automatically start receiving during connection.
        }

        protected override void OnSent(long sent, long pending)
        {
            base.OnSent(sent, pending);
            this.timer.Stop();
            this.timer.Start();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            this.timer.Stop();
            base.OnReceived(buffer, offset, size);
            EndPoint remoteEndpoint = this.Socket.RemoteEndPoint;
            this.Received?.Invoke(this, remoteEndpoint, buffer, offset, size);
            // base.ReceiveAsync();  // TcpSession will try to receive again after exiting base.OnReceived
            this.timer.Start();
        }

        protected override void OnDisconnected()
        {
            this.timer.Stop();
            base.OnDisconnected();
            this.Disconnected?.Invoke(this);
        }
    }
}
