using SocketBackendFramework.Relay.Models.Transport.PacketContexts;

namespace SocketBackendFramework.Relay.Models.Transport.Listeners
{
    public class ListenerConfig
    {
        public int ListeningPort { get; set; }
        public string? TransportType { get; set; }
        public double TcpSessionTimeoutMs { get; set; }
        public string? SocketHandlerConfigId { get; set; }
    }
}
