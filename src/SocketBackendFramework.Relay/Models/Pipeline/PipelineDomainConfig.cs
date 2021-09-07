using SocketBackendFramework.Relay.Models.Transport;

namespace SocketBackendFramework.Relay.Models.Pipeline
{
    public class PipelineDomainConfig
    {
        public string Name { get; set; } = "undefined";
        public TransportMapperConfig TransportMapper { get; set; } = new();
    }
}
