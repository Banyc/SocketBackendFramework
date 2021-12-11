using System;
using System.Net;
using SocketBackendFramework.Relay.Models.Transport.Listeners;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
using SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers;

namespace SocketBackendFramework.Relay.Transport.Listeners
{
    public class Listener : ITransportAgent, IDisposable
    {
        public event EventHandler<DownwardPacketContext>? PacketReceived;
        public event EventHandler<DownwardPacketContext>? TcpServerConnected;
        public event EventHandler<DownwardPacketContext>? TcpSessionDisconnected;

        private readonly ListenerConfig config;
        private readonly IServerHandler server;

        public uint TransportAgentId { get; }

        public Listener(ListenerConfig config, IServerHandlerBuilder builder, uint transportAgentId)
        {
            this.config = config;
            this.TransportAgentId = transportAgentId;

            // build system socket
            // don't start listening yet
            this.server = builder.Build(new IPEndPoint(IPAddress.Any, config.ListeningPort), config.SocketHandlerConfigId);
            this.server.ClientConnected += OnTcpServerConnected;
            this.server.ClientDisconnected += OnTcpSessionDisconnected;
            this.server.ClientMessageReceived += OnReceive;
        }

        public void Start()
        {
            // activate socket
            this.server.Start();
        }

        public void Respond(UpwardPacketContext context)
        {
            this.server.Send(context.FiveTuples!.Remote!, context.PacketRawBuffer!, context.PacketRawOffset, context.PacketRawSize);
        }

        public void DisconnectTcpSession(UpwardPacketContext context)
        {
            // disconnect a TCP session, not the listener
            this.server.Disconnect(context.FiveTuples!.Remote!);
        }

        private void OnTcpServerConnected(object sender, string transportType, EndPoint localEndPoint, EndPoint remoteEndPoint)
        {
            this.TcpServerConnected?.Invoke(this, new()
            {
                EventType = DownwardEventType.TcpServerConnected,
                FiveTuples = new()
                {
                    Local = (IPEndPoint)localEndPoint,
                    Remote = (IPEndPoint)remoteEndPoint,
                    TransportType = transportType,
                },
                TransportAgentId = this.TransportAgentId,
            });
        }

        private void OnTcpSessionDisconnected(object sender, string transportType, EndPoint localEndPoint, EndPoint remoteEndPoint)
        {
            this.TcpSessionDisconnected?.Invoke(this, new()
            {
                EventType = DownwardEventType.Disconnected,
                FiveTuples = new()
                {
                    Local = (IPEndPoint)localEndPoint,
                    Remote = (IPEndPoint)remoteEndPoint,
                    TransportType = transportType,
                },
                TransportAgentId = this.TransportAgentId,
            });
        }

        private void OnReceive(object sender, string transportType, EndPoint localEndPoint, EndPoint remoteEndPoint, byte[] buffer, long offset, long size)
        {
            DownwardPacketContext context = new()
            {
                EventType = DownwardEventType.ApplicationMessageReceived,
                FiveTuples = new()
                {
                    Local = (IPEndPoint)localEndPoint,
                    Remote = (IPEndPoint)remoteEndPoint,
                    TransportType = transportType,
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
            this.server.Dispose();
        }
    }
}
