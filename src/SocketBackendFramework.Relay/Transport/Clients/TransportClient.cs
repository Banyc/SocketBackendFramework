using System.Net;
using SocketBackendFramework.Relay.Models;
using SocketBackendFramework.Relay.Models.Transport.Clients;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;
using static SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers.TcpSessionHandler;

namespace SocketBackendFramework.Relay.Transport.Clients
{
    public class TransportClient : ITransportAgent, IDisposable
    {
        public event EventHandler<PacketContext> PacketReceived;

        // tell transport mapper to dispose this
        public event DisconnectedEventHandler TcpClientDisconnected;
        public event DisconnectedEventHandler ClientTimedOut;

        public int LocalPort
        {
            get
            {
                switch (this.config.TransportType)
                {
                    case Models.Transport.Listeners.ExclusiveTransportType.Tcp:
                        return ((IPEndPoint)this.tcpClient.Socket.LocalEndPoint).Port;
                        break;
                    case Models.Transport.Listeners.ExclusiveTransportType.Udp:
                        return ((IPEndPoint)this.udpClient.Socket.LocalEndPoint).Port;
                        break;
                    default:
                        return -1;
                        break;
                }
            }
        }

        private readonly System.Timers.Timer timer;
        private readonly TcpClientHandler? tcpClient;
        private readonly UdpClientHandler? udpClient;

        private TransportClientConfig config;

        public TransportClient(TransportClientConfig config)
        {
            this.config = config;

            // build client
            switch (config.TransportType)
            {
                case Models.Transport.Listeners.ExclusiveTransportType.Tcp:
                    this.tcpClient = new(config.RemoteAddress, config.RemotePort);
                    this.tcpClient.Disconnected += TcpClientDisconnected;
                    this.tcpClient.Received += OnReceive;
                    this.tcpClient.Connect();
                    break;
                case Models.Transport.Listeners.ExclusiveTransportType.Udp:
                    this.udpClient = new(config.RemoteAddress, config.RemotePort);
                    this.udpClient.Received += OnReceive;
                    this.udpClient.Connect();
                    break;
            }

            this.timer = new()
            {
                Interval = config.ClientDisposeTimeout.TotalMilliseconds,
            };
            this.timer.Elapsed += (sender, e) => this.ClientTimedOut?.Invoke(this);
        }

        public void Respond(PacketContext context)
        {
            this.timer.Stop();
            switch (this.config.TransportType)
            {
                case Models.Transport.Listeners.ExclusiveTransportType.Tcp:
                    this.tcpClient.SendAfterConnected(context.ResponsePacketRaw.ToArray());
                    break;
                case Models.Transport.Listeners.ExclusiveTransportType.Udp:
                    this.udpClient.Send(context.ResponsePacketRaw.ToArray());
                    break;
            }
            this.timer.Start();
        }

        public void Dispose()
        {
            this.timer.Dispose();
            this.tcpClient?.Dispose();
            this.udpClient?.Dispose();
        }

        private void OnReceive(object sender, EndPoint remoteEndpoint, byte[] buffer, long offset, long size)
        {
            this.timer.Stop();
            IPEndPoint remoteIPEndPoint = (IPEndPoint)remoteEndpoint;
            PacketContext context = new()
            {
                LocalPort = this.LocalPort,
                RemoteIp = remoteIPEndPoint.Address,
                RemotePort = remoteIPEndPoint.Port,
                RequestPacketRawBuffer = buffer,
                RequestPacketRawOffset = offset,
                RequestPacketRawSize = size,
            };
            this.PacketReceived?.Invoke(this, context);
            this.timer.Start();
        }
    }
}
