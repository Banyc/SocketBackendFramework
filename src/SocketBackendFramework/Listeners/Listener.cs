using System.Collections.Generic;
using System.Net;
using SocketBackendFramework.Listeners.SocketHandlers;
using SocketBackendFramework.Models;
using SocketBackendFramework.Models.Listeners;

namespace SocketBackendFramework.Listeners
{
    public class Listener
    {
        private readonly ListenerConfig config;
        private readonly ListenersMapper mapper;
        private readonly TcpServerHandler tcpServer;
        private readonly Dictionary<int, TcpSessionHandler> tcpSessions = new();
        private readonly UdpServerHandler udpServer;

        public Listener(ListenerConfig config, ListenersMapper mapper)
        {
            this.config = config;
            this.mapper = mapper;

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
            if (!context.ShouldRespond)
            {
                return;
            }
            switch (config.TransportType)
            {
                case ExclusiveTransportType.Tcp:
                    this.tcpSessions[context.RemotePort].Send(context.ResponsePacketRaw.ToArray());
                    break;
                case ExclusiveTransportType.Udp:
                    this.udpServer.Send(context.ResponsePacketRaw.ToArray());
                    break;
            }
        }

        private void OnTcpServerConnected(object sender, TcpSessionHandler session)
        {
            IPEndPoint remoteEndPoint = (IPEndPoint)session.Socket.RemoteEndPoint;
            int remotePort = remoteEndPoint.Port;
            this.tcpSessions[remotePort] = session;
            session.Received += (object sender, byte[] buffer, long offset, long size) =>
            {
                this.OnReceive(sender, remoteEndPoint, buffer, offset, size);
            };
            session.Disconnected += OnTcpSessionDisconnected;
        }

        private void OnTcpSessionDisconnected(object sender)
        {
            TcpSessionHandler session = (TcpSessionHandler)sender;
            IPEndPoint remoteEndPoint = (IPEndPoint)session.Socket.RemoteEndPoint;
            int remotePort = remoteEndPoint.Port;
            this.tcpSessions.Remove(remotePort);
        }

        private void OnReceive(object sender, EndPoint remoteEndpoint, byte[] buffer, long offset, long size)
        {
            IPEndPoint remoteIPEndPoint = (IPEndPoint)remoteEndpoint;
            PacketContext context = new()
            {
                LocalPort = this.config.ListeningPort,
                RemoteIp = remoteIPEndPoint.Address,
                RemotePort = remoteIPEndPoint.Port,
                RequestPacketRawBuffer = buffer,
                RequestPacketRawOffset = offset,
                RequestPacketRawSize = size,
            };
            this.mapper.OnReceivePacket(context);
        }
    }
}
