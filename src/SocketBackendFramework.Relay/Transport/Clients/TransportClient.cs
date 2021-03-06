using System;
using System.Net;
using SocketBackendFramework.Relay.Models.Delegates;
using SocketBackendFramework.Relay.Models.Transport.Clients;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;

namespace SocketBackendFramework.Relay.Transport.Clients
{
    public class TransportClient : ITransportAgent, IDisposable
    {
        public event EventHandler<DownwardPacketContext>? PacketReceived;

        // tell transport mapper when this object's local port is available.
        public event ConnectionEventHandler? Connected;

        // tell transport mapper to dispose this
        public event EventHandler<DownwardPacketContext>? Disconnected;

        // in case this info cannot be accessed from a disposed socket object
        public IPEndPoint? LocalIPEndPoint { get; private set; }

        public uint TransportAgentId { get; }

        private readonly System.Timers.Timer timer;
        private readonly IClientHandler client;

        private readonly TransportClientConfig config;

        public TransportClient(TransportClientConfig config, IClientHandlerBuilder builder, uint transportAgentId)
        {
            this.config = config;
            this.TransportAgentId = transportAgentId;

            // build client
            this.client = builder.Build(new IPEndPoint(IPAddress.Parse(config.RemoteAddress!), config.RemotePort), config.SocketHandlerConfig);
            this.client.Connected += (sender, transportType, localEndPoint, remoteEndPoint) =>
            {
                IClientHandler client = (IClientHandler)sender;
                this.LocalIPEndPoint = (IPEndPoint)client.LocalEndPoint!;
                this.Connected?.Invoke(
                    this,
                    transportType,
                    localEndPoint,
                    remoteEndPoint);
            };
            this.client.Received += OnReceive;
            this.client.Disconnected += OnDisconnected;

            this.timer = new()
            {
                Interval = config.ClientDisposeTimeout.TotalMilliseconds,
                AutoReset = false,
            };
            this.timer.Elapsed += (sender, e) => this.DisconnectAsync();
        }

        public void Connect()
        {
            this.client.Connect();
            this.timer.Start();
        }

        public void Respond(UpwardPacketContext context)
        {
            this.timer.Stop();
            this.client.Send(context.PacketRawBuffer!, context.PacketRawOffset, context.PacketRawSize);
            this.timer.Start();
        }

        public void DisconnectAsync()
        {
            this.timer.Stop();
            this.client.Disconnect();
        }

        public void Dispose()
        {
            // Console.WriteLine("TransportAgent {} has been disposed.");
            this.timer.Dispose();
            this.client.Dispose();
            GC.SuppressFinalize(this);
        }

        private void OnDisconnected(object sender, string transportType, EndPoint localEndPoint, EndPoint remoteEndPoint)
        {
            this.timer.Stop();
            this.Disconnected?.Invoke(this, new()
            {
                EventType = DownwardEventType.Disconnected,
                FiveTuples = new()
                {
                    Local = this.LocalIPEndPoint,
                    Remote = new(IPAddress.Parse(this.config.RemoteAddress!),
                                 this.config.RemotePort),
                    TransportType = this.config.TransportType,
                },
                TransportAgentId = this.TransportAgentId,
            });
        }

        private void OnReceive(object sender, string transportType, EndPoint localEndPoint, EndPoint remoteEndpoint, byte[] buffer, long offset, long size)
        {
            this.timer.Stop();
            IPEndPoint remoteIPEndPoint = (IPEndPoint)remoteEndpoint;
            this.PacketReceived?.Invoke(this, new()
            {
                EventType = DownwardEventType.ApplicationMessageReceived,
                FiveTuples = new()
                {
                    Local = this.LocalIPEndPoint,
                    Remote = remoteIPEndPoint,
                    TransportType = this.config.TransportType,
                },
                TransportAgentId = this.TransportAgentId,
                PacketRawBuffer = buffer,
                PacketRawOffset = offset,
                PacketRawSize = size,
            });
            this.timer.Start();
        }
    }
}
