using SocketBackendFramework.Relay.Models.Transport.Listeners.SocketHandlers;
using SocketBackendFramework.Relay.Models.Workflows;

namespace SocketBackendFramework.Relay.Sample.Models
{
    public class ConfigRoot
    {
        public WorkflowPoolConfig? WorkflowPool { get; set; }
        public TcpServerHandlerBuilderConfig? TcpServerHandlerBuilder { get; set; }
    }
}
