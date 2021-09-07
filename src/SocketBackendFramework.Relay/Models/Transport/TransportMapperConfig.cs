using SocketBackendFramework.Relay.Models.Transport.Listeners;

namespace SocketBackendFramework.Relay.Models.Transport
{
    public class TransportMapperConfig
    {
        public List<ListenerConfig> Listeners { get; set; } = new();
    }
}
