using SocketBackendFramework.Models.Listeners;

namespace SocketBackendFramework.Models.Workflows
{
    public class WorkflowConfig
    {
        public ListenersMapperConfig ListenersMapperConfig { get; set; }
        public string Name { get; set; }
    }
}
