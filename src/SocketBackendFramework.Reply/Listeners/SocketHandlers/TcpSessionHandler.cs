using NetCoreServer;

namespace SocketBackendFramework.Reply.Listeners.SocketHandlers
{
    public class TcpSessionHandler : TcpSession
    {
        public delegate void ReceivedEventHandler(object sender, byte[] buffer, long offset, long size);
        public event ReceivedEventHandler Received;

        public delegate void DisconnectedEventHandler(object sender);
        public event DisconnectedEventHandler Disconnected;

        public TcpSessionHandler(TcpServer server) : base(server)
        {
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            base.OnReceived(buffer, offset, size);
            this.Received?.Invoke(this, buffer, offset, size);
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            this.Disconnected?.Invoke(this);
        }
    }
}
