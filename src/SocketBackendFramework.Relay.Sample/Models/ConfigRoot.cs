using SocketBackendFramework.Relay.Models.Transport.Listeners.SocketHandlers;
using SocketBackendFramework.Relay.Models.Workflows;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Controllers;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.DefaultSocketHandlers;

namespace SocketBackendFramework.Relay.Sample.Models
{
    public class ConfigRoot
    {
        public WorkflowPoolConfig? WorkflowPool { get; set; }
        public TcpServerHandlerBuilderConfig? TcpServerHandlerBuilder { get; set; }
        public KcpServerHandlerBuilderConfig? KcpServerHandlerBuilder { get; set; }
        public RelayControllerConfig? RelayController { get; set; }
    }
}
