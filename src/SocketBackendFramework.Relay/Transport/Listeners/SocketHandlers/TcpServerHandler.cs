using System.Net;
using NetCoreServer;

namespace SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers
{
    public class TcpServerHandler : TcpServer
    {
        private readonly double tcpSessionTimeoutMs;

        public TcpServerHandler(IPAddress address, int port, double tcpSessionTimeoutMs) : base(address, port)
        {
            this.tcpSessionTimeoutMs = tcpSessionTimeoutMs;
        }

        public event EventHandler<TcpSessionHandler> Connected;

        protected override void OnConnected(TcpSession session)
        {
            base.OnConnected(session);
            this.Connected?.Invoke(this, (TcpSessionHandler)session);
        }

        protected override TcpSession CreateSession()
        {
            TcpSessionHandler tcpSession = new(this, this.tcpSessionTimeoutMs);
            return tcpSession;
        }
    }
}
