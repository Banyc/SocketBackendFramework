using System.Net;

namespace SocketBackendFramework.Relay.Models.Transport.PacketContexts
{
    public record FiveTuples
    {
        public IPAddress RemoteIp { get; set; }
        public int RemotePort { get; set; }
        public IPAddress LocalIp { get; set; }
        public int LocalPort { get; set; }
        public ExclusiveTransportType TransportType { get; set; }
    }
}
