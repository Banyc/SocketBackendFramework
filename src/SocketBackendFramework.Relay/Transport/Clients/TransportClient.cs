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

        // tell transport mapper to dispose this
        public event DisconnectedEventHandler TcpClientDisconnected;

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
                    break;
                case Models.Transport.Listeners.ExclusiveTransportType.Udp:
                    this.udpClient = new(config.RemoteAddress, config.RemotePort);
                    this.udpClient.Received += OnReceive;
                    break;
            }
        }

        public void Respond(PacketContext context)
        {
            switch (this.config.TransportType)
            {
                case Models.Transport.Listeners.ExclusiveTransportType.Tcp:
                    this.tcpClient.SendAfterConnected(context.ResponsePacketRaw.ToArray());
                    break;
                case Models.Transport.Listeners.ExclusiveTransportType.Udp:
                    this.udpClient.Send(context.ResponsePacketRaw.ToArray());
                    break;
            }
        }

        public void Dispose()
        {
            this.tcpClient.Dispose();
        }

        private void OnReceive(object sender, EndPoint remoteEndpoint, byte[] buffer, long offset, long size)
        {
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
        }
    }
}
