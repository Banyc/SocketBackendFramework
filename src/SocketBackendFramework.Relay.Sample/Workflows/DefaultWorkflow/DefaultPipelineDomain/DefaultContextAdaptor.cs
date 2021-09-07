using SocketBackendFramework.Relay.ContextAdaptor;
using SocketBackendFramework.Relay.Models;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain
{
    public class DefaultContextAdaptor : IContextAdaptor<DefaultMiddlewareContext>
    {
        public DefaultMiddlewareContext GetMiddlewareContext(PacketContext packetContext)
        {
            return new()
            {
                PacketContext = packetContext,
            };
        }

        public PacketContext GetPacketContext(DefaultMiddlewareContext middlewareContext)
        {
            return middlewareContext.PacketContext;
        }
    }
}
