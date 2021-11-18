using System;
using System.Collections.Concurrent;
using System.Net;
using NetCoreServer;
using SocketBackendFramework.Relay.Models.Delegates;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;

namespace SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers
{
    public class TcpServerHandler : TcpServer, IServerHandler
    {
        private readonly double tcpSessionTimeoutMs;

        // remote endpoint -> session
        // private readonly ConcurrentDictionary<EndPoint, TcpSessionHandler> tcpSessions = new();
        private readonly ConcurrentDictionary<EndPoint, IClientHandler> tcpSessions = new();

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
            this.ClientConnected?.Invoke(
                this,
                client.TransportType,
                client.LocalEndPoint,
                client.RemoteEndPoint);
            client.Disconnected += (sender, transportType, localEndPoint, remoteEndPoint) =>
                this.ClientDisconnected?.Invoke(
                    this,
                    transportType,
                    localEndPoint,
                    remoteEndPoint);
            client.Received += (sender, transportType, localEndPoint, remoteEndPoint, buffer, offset, size) =>
                this.ClientMessageReceived?.Invoke(
                    this,
                    transportType,
                    localEndPoint,
                    remoteEndPoint,
                    buffer, offset, size);
            this.tcpSessions[client.RemoteEndPoint] = client;
        }

        protected override TcpSession CreateSession()
        {
            TcpSessionHandler tcpSession = new(this, this.tcpSessionTimeoutMs);
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
            this.tcpSessions[remoteEndPoint].Send(buffer, offset, size);
        }

        public void Disconnect(EndPoint remoteEndPoint)
        {
            this.tcpSessions[remoteEndPoint].Disconnect();
            this.tcpSessions[remoteEndPoint].Dispose();
            this.tcpSessions.TryRemove(remoteEndPoint, out _);
        }
        #endregion
    }
}
