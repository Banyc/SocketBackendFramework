using System;
using System.Net;
using SocketBackendFramework.Relay.Models;
using SocketBackendFramework.Relay.Models.Delegates;
using SocketBackendFramework.Relay.Models.Transport;
using SocketBackendFramework.Relay.Models.Transport.Clients;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;

namespace SocketBackendFramework.Relay.Transport.Clients
{
    public class TransportClient : ITransportAgent, IDisposable
    {
        public event EventHandler<PacketContext> PacketReceived;

        // tell transport mapper when this object's local port is available.
        public event SimpleEventHandler Connected;

        // tell transport mapper to dispose this
        public event EventHandler<PacketContext> Disconnected;

        // tell transport mapper to disconnect this
        public event SimpleEventHandler ClientTimedOut;

        // in case this info cannot be accessed from a disposed socket object
        public IPEndPoint LocalIPEndPoint { get; private set; }

        public uint TransportAgentId { get; }

        private readonly System.Timers.Timer timer;
        private readonly TcpClientHandler? tcpClient;
        private readonly UdpClientHandler? udpClient;

        private TransportClientConfig config;

        public TransportClient(TransportClientConfig config, uint transportAgentId)
        {
            this.config = config;
            this.TransportAgentId = transportAgentId;

            // build client
            switch (config.TransportType)
            {
                case ExclusiveTransportType.Tcp:
                    this.tcpClient = new(config.RemoteAddress, config.RemotePort);
                    this.tcpClient.Connected += sender =>
                    {
                        TcpClientHandler tcpClient = (TcpClientHandler)sender;
                        this.LocalIPEndPoint = (IPEndPoint)tcpClient.Socket.LocalEndPoint;
                        this.Connected?.Invoke(this);
                    };
                    this.tcpClient.Received += OnReceive;
                    this.tcpClient.Disconnected += OnDisconnected;
                    this.tcpClient.ConnectAsync();
                    break;
                case ExclusiveTransportType.Udp:
                    this.udpClient = new(config.RemoteAddress, config.RemotePort);
                    this.udpClient.Connected += sender =>
                    {
                        UdpClientHandler udpClient = (UdpClientHandler)sender;
                        this.LocalIPEndPoint = (IPEndPoint)udpClient.Socket.LocalEndPoint;
                        this.Connected?.Invoke(this);
                    };
                    this.udpClient.Received += OnReceive;
                    this.udpClient.Disconnected += OnDisconnected;
                    this.udpClient.Connect();
                    break;
            }

            this.timer = new()
            {
                Interval = config.ClientDisposeTimeout.TotalMilliseconds,
                AutoReset = false,
            };
            this.timer.Elapsed += (sender, e) => this.ClientTimedOut?.Invoke(this);
            this.timer.Start();
        }

        public void Respond(PacketContext context)
        {
            this.timer.Stop();
            switch (this.config.TransportType)
            {
                case ExclusiveTransportType.Tcp:
                    this.tcpClient.SendAfterConnected(context.ResponsePacketRaw.ToArray());
                    break;
                case ExclusiveTransportType.Udp:
                    this.udpClient.Send(context.ResponsePacketRaw.ToArray());
                    break;
            }
            this.timer.Start();
        }

        public void DisconnectAsync()
        {
            this.timer.Stop();
            switch (this.config.TransportType)
            {
                case ExclusiveTransportType.Tcp:
                    this.tcpClient.DisconnectAsync();
                    break;
                case ExclusiveTransportType.Udp:
                    this.udpClient.Disconnect();
                    break;
                default:
                    throw new ArgumentException();
                    break;
            }
        }

        public void Dispose()
        {
            // Console.WriteLine("TransportAgent {} has been disposed.");
            this.timer.Dispose();
            this.tcpClient?.Dispose();
            this.udpClient?.Dispose();
        }

        private void OnDisconnected(object sender)
        {
            this.timer.Stop();
            this.Disconnected?.Invoke(this, new()
            {
                PacketContextType = PacketContextType.Disconnecting,
                LocalIp = this.LocalIPEndPoint.Address,
                LocalPort = this.LocalIPEndPoint.Port,
                RemoteIp = IPAddress.Parse(this.config.RemoteAddress),
                RemotePort = this.config.RemotePort,
                TransportAgentId = this.TransportAgentId,
                TransportType = this.config.TransportType,
            });
        }

        private void OnReceive(object sender, EndPoint remoteEndpoint, byte[] buffer, long offset, long size)
        {
            this.timer.Stop();
            IPEndPoint remoteIPEndPoint = (IPEndPoint)remoteEndpoint;
            PacketContext context = new()
            {
                PacketContextType = PacketContextType.ApplicationMessaging,
                LocalIp = this.LocalIPEndPoint.Address,
                LocalPort = this.LocalIPEndPoint.Port,
                RemoteIp = remoteIPEndPoint.Address,
                RemotePort = remoteIPEndPoint.Port,
                TransportAgentId = this.TransportAgentId,
                TransportType = this.config.TransportType,
                RequestPacketRawBuffer = buffer,
                RequestPacketRawOffset = offset,
                RequestPacketRawSize = size,
            };
            this.PacketReceived?.Invoke(this, context);
            this.timer.Start();
        }
    }
}
