using System.Net;

namespace SocketBackendFramework.Relay.Models.Transport.PacketContexts
{
    public record FiveTuples
    {
        public IPEndPoint Remote { get; init; }
        public IPEndPoint Local { get; init; }
        public ExclusiveTransportType TransportType { get; init; }
    }
}
