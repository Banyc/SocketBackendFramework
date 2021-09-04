using SocketBackendFramework.Middlewares.ContextAdaptor;
using SocketBackendFramework.Models;
using SocketBackendFramework.Models.Middlewares;
using SocketBackendFramework.Sample.Models;

namespace SocketBackendFramework.Sample.Helpers
{
    public class ContextAdaptor : IContextAdaptor<MiddlewareContext>
    {
        public MiddlewareContext GetMiddlewareContext(PacketContext packetContext)
        {
            return new MiddlewareContext()
            {
                PacketContext = packetContext
            };
        }

        public PacketContext GetPacketContext(MiddlewareContext middlewareContext)
        {
            return middlewareContext.PacketContext;
        }
    }
}
