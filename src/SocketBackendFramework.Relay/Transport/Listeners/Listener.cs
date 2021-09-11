using System.Net;
using SocketBackendFramework.Relay.Models;
using SocketBackendFramework.Relay.Models.Transport;
using SocketBackendFramework.Relay.Models.Transport.Listeners;
using SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers;

namespace SocketBackendFramework.Relay.Transport.Listeners
{
    public class Listener : ITransportAgent, IDisposable
    {
        public event EventHandler<PacketContext> PacketReceived;
        public event EventHandler<PacketContext> TcpSessionDisconnected;

        private readonly ListenerConfig config;
        private readonly TcpServerHandler tcpServer;

        // remote port -> tcp session
        private readonly Dictionary<int, TcpSessionHandler> tcpSessions = new();

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
                    this.tcpServer = new TcpServerHandler(IPAddress.Any, config.ListeningPort);
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

        public void Respond(PacketContext context)
        {
            switch (config.TransportType)
            {
                case ExclusiveTransportType.Tcp:
                    this.tcpSessions[context.RemotePort].Send(context.ResponsePacketRaw.ToArray());
                    break;
                case ExclusiveTransportType.Udp:
                    this.udpServer.Send(new IPEndPoint(context.RemoteIp, context.RemotePort),
                                        context.ResponsePacketRaw.ToArray());
                    break;
            }
        }

        public void DisconnectTcpSession(PacketContext context)
        {
            switch (config.TransportType)
            {
                case ExclusiveTransportType.Tcp:
                    // disconnect a TCP session, not the listener
                    this.tcpSessions[context.RemotePort].Disconnect();
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
            session.Disconnected += OnTcpSessionDisconnected;
        }

        private void OnTcpSessionDisconnected(object sender)
        {
            TcpSessionHandler session = (TcpSessionHandler)sender;
            this.TcpSessionDisconnected?.Invoke(this, new()
            {
                PacketContextType = PacketContextType.Disconnecting,
                LocalIp = session.LocalIPEndPoint.Address,
                LocalPort = this.config.ListeningPort,
                RemoteIp = session.RemoteIPEndPoint.Address,
                RemotePort = session.RemoteIPEndPoint.Port,
                TransportAgentId = this.TransportAgentId,
                TransportType = ExclusiveTransportType.Tcp,
            });
            this.tcpSessions.Remove(session.RemoteIPEndPoint.Port);
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
            PacketContext context = new()
            {
                PacketContextType = PacketContextType.ApplicationMessaging,
                LocalIp = localIPEndPoint.Address,
                LocalPort = this.config.ListeningPort,
                RemoteIp = remoteIPEndPoint.Address,
                RemotePort = remoteIPEndPoint.Port,
                TransportAgentId = this.TransportAgentId,
                TransportType = this.config.TransportType,
                RequestPacketRawBuffer = buffer,
                RequestPacketRawOffset = offset,
                RequestPacketRawSize = size,
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
