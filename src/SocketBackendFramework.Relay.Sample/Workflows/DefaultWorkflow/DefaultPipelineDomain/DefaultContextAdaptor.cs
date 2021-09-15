using SocketBackendFramework.Relay.ContextAdaptor;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain
{
    public class DefaultContextAdaptor : IContextAdaptor<DefaultMiddlewareContext>
    {
        public DefaultMiddlewareContext GetMiddlewareContext(DownwardPacketContext packetContext)
        {
            return new()
            {
                Request = new()
                {
                    PacketContext = packetContext,
                },
            };
        }

        public UpwardPacketContext GetPacketContext(DefaultMiddlewareContext middlewareContext)
        {
            return middlewareContext.Response.PacketContext;
        }
    }
}
