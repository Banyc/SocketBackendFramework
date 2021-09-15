using System.Collections.Generic;
using System.Net;
using SocketBackendFramework.Relay.Models.Transport;
using SocketBackendFramework.Relay.Models.Transport.Clients;

namespace SocketBackendFramework.Relay.Models
{
    public enum PacketContextType
    {
        // ServerStarting,
        // TcpServerAccepting,

        // not allow to return this type of PacketContext to transport layer.
        TcpServerConnection,

        // when return PacketContext to transport layer,
        //   the transport mapper will disconnect the agent with the five-tuple.
        Disconnection,

        ApplicationMessage,
    }

    public class PacketContext
    {
        // the five tuples
        public IPAddress RemoteIp { get; set; }
        public int RemotePort { get; set; }
        public IPAddress LocalIp { get; set; }
        public int LocalPort { get; set; }
        public ExclusiveTransportType TransportType { get; set; }

        // ignored when the packetContext return to transport layer.
        public uint TransportAgentId { get; set; }

        public PacketContextType PacketContextType { get; set; }

        public byte[]? RequestPacketRawBuffer { get; set; }
        public long RequestPacketRawOffset { get; set; }
        public long RequestPacketRawSize { get; set; }
        public List<byte> ResponsePacketRaw { get; set; } = new();

        // if null, create a dedicated socket client
        public TransportClientConfig? ClientConfig { get; set; }
    }
}
