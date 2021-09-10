using SocketBackendFramework.Relay.Models.Transport;

namespace SocketBackendFramework.Relay.Models.Transport.Listeners
{
    public class ListenerConfig
    {
        public int ListeningPort { get; set; }
        public ExclusiveTransportType TransportType { get; set; }
    }
}
