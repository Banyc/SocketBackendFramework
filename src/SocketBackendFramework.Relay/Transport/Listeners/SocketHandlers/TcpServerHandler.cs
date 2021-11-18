using System;
using System.Collections.Concurrent;
using System.Net;
using System.Timers;
using NetCoreServer;
using SocketBackendFramework.Relay.Models.Delegates;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;

namespace SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers
{
    public class TcpServerHandler : TcpServer, IServerHandler
    {
        private struct ClientInfo
        {
            public IClientHandler ClientHandler;
            public Timer TimeoutTimer;
        }
        private readonly double tcpSessionTimeoutMs;

        // remote endpoint -> session
        // private readonly ConcurrentDictionary<EndPoint, TcpSessionHandler> tcpSessions = new();
        private readonly ConcurrentDictionary<EndPoint, ClientInfo> tcpSessions = new();

        public string TransportType => "tcp";

        public EndPoint LocalEndPoint { get; }

        public TcpServerHandler(IPAddress address, int port, double tcpSessionTimeoutMs) : base(address, port)
        {
            this.tcpSessionTimeoutMs = tcpSessionTimeoutMs;
            this.LocalEndPoint = new IPEndPoint(address, port);
        }

        public event ConnectionEventHandler ClientConnected;
        public event ConnectionEventHandler ClientDisconnected;
        public event ReceivedEventHandler ClientMessageReceived;

        #region TcpServer overrides
        protected override void OnConnected(TcpSession session)
        {
            IClientHandler client = (IClientHandler)session;
            Timer timeoutTimer = new Timer(this.tcpSessionTimeoutMs);
            this.tcpSessions[client.RemoteEndPoint] = new()
            {
                ClientHandler = client,
                TimeoutTimer = timeoutTimer,
            };

            // register callbacks
            timeoutTimer.Elapsed += (sender, e) =>
            {
                this.Disconnect(client.RemoteEndPoint);
            };
            client.Disconnected += (sender, transportType, localEndPoint, remoteEndPoint) =>
            {
                this.Disconnect(client.RemoteEndPoint);  
                this.ClientDisconnected?.Invoke(
                    this,
                    transportType,
                    localEndPoint,
                    remoteEndPoint);
            };
            client.Received += (sender, transportType, localEndPoint, remoteEndPoint, buffer, offset, size) =>
            {
                timeoutTimer.Stop();
                this.ClientMessageReceived?.Invoke(
                    this,
                    transportType,
                    localEndPoint,
                    remoteEndPoint,
                    buffer, offset, size);
                timeoutTimer.Start();
            };

            timeoutTimer.Start();
            this.ClientConnected?.Invoke(
                this,
                client.TransportType,
                client.LocalEndPoint,
                client.RemoteEndPoint);
        }

        protected override TcpSession CreateSession()
        {
            TcpSessionHandler tcpSession = new(this);
            return tcpSession;
        }
        #endregion

        #region IServerHandler
        void IServerHandler.Start()
        {
            base.Start();
        }

        void IServerHandler.Send(EndPoint remoteEndPoint, byte[] buffer, long offset, long size)
        {
            var clientInfo = this.tcpSessions[remoteEndPoint];
            clientInfo.TimeoutTimer.Stop();
            clientInfo.ClientHandler.Send(buffer, offset, size);
            clientInfo.TimeoutTimer.Start();
        }

        public void Disconnect(EndPoint remoteEndPoint)
        {
            var clientInfo = this.tcpSessions[remoteEndPoint];
            clientInfo.TimeoutTimer.Stop();
            clientInfo.TimeoutTimer.Dispose();
            clientInfo.ClientHandler.Disconnect();
            clientInfo.ClientHandler.Dispose();
            this.tcpSessions.TryRemove(remoteEndPoint, out _);
        }
        #endregion
    }
}
