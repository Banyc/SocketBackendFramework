using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using SocketBackendFramework.Relay.Models.Transport.Listeners;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
using SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers;

namespace SocketBackendFramework.Relay.Transport.Listeners
{
    public class Listener : ITransportAgent, IDisposable
    {
        public event EventHandler<DownwardPacketContext> PacketReceived;
        public event EventHandler<DownwardPacketContext> TcpServerConnected;
        public event EventHandler<DownwardPacketContext> TcpSessionDisconnected;

        private readonly ListenerConfig config;
        private readonly TcpServerHandler tcpServer;

        // remote port -> tcp session
        private readonly ConcurrentDictionary<int, TcpSessionHandler> tcpSessions = new();

        private readonly UdpServerHandler udpServer;

        public uint TransportAgentId { get; }

        public Listener(ListenerConfig config, uint transportAgentId)
        {
            this.config = config;
            this.TransportAgentId = transportAgentId;

            // build system socket
            // don't start listening yet
            switch (config.TransportType)
            {
                case ExclusiveTransportType.Tcp:
                    this.tcpServer = new TcpServerHandler(IPAddress.Any, config.ListeningPort, config.TcpSessionTimeoutMs);
                    this.tcpServer.Connected += OnTcpServerConnected;
                    break;
                case ExclusiveTransportType.Udp:
                    this.udpServer = new UdpServerHandler(IPAddress.Any, config.ListeningPort);
                    this.udpServer.Received += OnReceive;
                    break;
            }
        }

        public void Start()
        {
            // activate socket
            switch (config.TransportType)
            {
                case ExclusiveTransportType.Tcp:
                    this.tcpServer.Start();
                    break;
                case ExclusiveTransportType.Udp:
                    this.udpServer.Start();
                    break;
            }
        }

        public void Respond(UpwardPacketContext context)
        {
            switch (config.TransportType)
            {
                case ExclusiveTransportType.Tcp:
                    this.tcpSessions[context.FiveTuples.Remote.Port]
                        .Send(context.PacketRawBuffer, context.PacketRawOffset, context.PacketRawSize);
                    break;
                case ExclusiveTransportType.Udp:
                    this.udpServer.Send(context.FiveTuples.Remote,
                                        context.PacketRawBuffer, context.PacketRawOffset, context.PacketRawSize);
                    break;
            }
        }

        public void DisconnectTcpSession(UpwardPacketContext context)
        {
            switch (config.TransportType)
            {
                case ExclusiveTransportType.Tcp:
                    // disconnect a TCP session, not the listener
                    this.tcpSessions[context.FiveTuples.Remote.Port].Disconnect();
                    break;
                case ExclusiveTransportType.Udp:
                    throw new ArgumentException("listeners are not allowed to disconnect");
                    break;
                default:
                    throw new ArgumentException();
                    break;
            }
        }

        private void OnTcpServerConnected(object sender, TcpSessionHandler session)
        {
            IPEndPoint remoteEndPoint = (IPEndPoint)session.Socket.RemoteEndPoint;
            int remotePort = remoteEndPoint.Port;
            this.tcpSessions[remotePort] = session;
            session.Received += this.OnReceive;
            session.TcpSessionTimedOut += sender =>
            {
                TcpSessionHandler tcpSession = (TcpSessionHandler)sender;
                tcpSession.Disconnect();
            };
            session.Disconnected += OnTcpSessionDisconnected;
            this.TcpServerConnected?.Invoke(this, new()
            {
                EventType = DownwardEventType.TcpServerConnected,
                FiveTuples = session.GetFiveTuples(),
                TransportAgentId = this.TransportAgentId,
            });
        }

        private void OnTcpSessionDisconnected(object sender)
        {
            TcpSessionHandler session = (TcpSessionHandler)sender;
            session.Dispose();
            this.TcpSessionDisconnected?.Invoke(this, new()
            {
                EventType = DownwardEventType.Disconnected,
                FiveTuples = session.GetFiveTuples(),
                TransportAgentId = this.TransportAgentId,
            });
            this.tcpSessions.Remove(session.RemoteIPEndPoint.Port, out _);
        }

        private void OnReceive(object sender, EndPoint remoteEndpoint, byte[] buffer, long offset, long size)
        {
            IPEndPoint remoteIPEndPoint = (IPEndPoint)remoteEndpoint;
            IPEndPoint localIPEndPoint;
            switch (this.config.TransportType)
            {
                case ExclusiveTransportType.Tcp:
                    localIPEndPoint = (IPEndPoint)this.tcpSessions[remoteIPEndPoint.Port].Socket.LocalEndPoint;
                    break;
                case ExclusiveTransportType.Udp:
                    localIPEndPoint = (IPEndPoint)this.udpServer.Socket.LocalEndPoint;
                    break;
                default:
                    throw new ArgumentException();
                    break;
            }
            #if DEBUG
            if (localIPEndPoint.Port != this.config.ListeningPort)
            {
                throw new Exception();
            }
            #endif
            DownwardPacketContext context = new()
            {
                EventType = DownwardEventType.ApplicationMessageReceived,
                FiveTuples = new()
                {
                    Local = localIPEndPoint,
                    Remote = remoteIPEndPoint,
                    TransportType = this.config.TransportType,
                },
                TransportAgentId = this.TransportAgentId,
                PacketRawBuffer = buffer,
                PacketRawOffset = offset,
                PacketRawSize = size,
            };
            this.PacketReceived?.Invoke(this, context);
        }

        public void Dispose()
        {
            foreach (var (_, session) in this.tcpSessions)
            {
                session.Dispose();
            }
            this.tcpServer?.Dispose();
            this.udpServer?.Dispose();
        }
    }
}
