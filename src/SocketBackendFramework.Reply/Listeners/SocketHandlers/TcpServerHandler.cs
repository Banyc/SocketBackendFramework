using System;
using System.Net;
using NetCoreServer;

namespace SocketBackendFramework.Reply.Listeners.SocketHandlers
{
    public class TcpServerHandler : TcpServer
    {
        public TcpServerHandler(IPAddress address, int port) : base(address, port)
        {
        }

        public event EventHandler<TcpSessionHandler> Connected;

        protected override void OnConnected(TcpSession session)
        {
            base.OnConnected(session);
            this.Connected?.Invoke(this, (TcpSessionHandler)session);
        }

        protected override TcpSession CreateSession()
        {
            TcpSessionHandler tcpSession = new(this);
            return tcpSession;
        }
    }
}
