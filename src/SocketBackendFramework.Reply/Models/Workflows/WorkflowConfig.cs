using SocketBackendFramework.Reply.Models.Listeners;

namespace SocketBackendFramework.Reply.Models.Workflows
{
    public class WorkflowConfig
    {
        public ListenersMapperConfig ListenersMapperConfig { get; set; }
        public string Name { get; set; }
    }
}
