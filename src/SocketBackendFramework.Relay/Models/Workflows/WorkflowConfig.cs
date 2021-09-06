using SocketBackendFramework.Relay.Models.Transport;

namespace SocketBackendFramework.Relay.Models.Workflows
{
    public class WorkflowConfig
    {
        // name -> object
        public Dictionary<string, TransportMapperConfig> TransportMapperConfigs { get; set; }
        public string Name { get; set; }
    }
}
