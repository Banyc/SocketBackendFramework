using SocketBackendFramework.Reply.Middlewares.ContextAdaptor;
using SocketBackendFramework.Reply.Models;
using SocketBackendFramework.Reply.Sample.Models;

namespace SocketBackendFramework.Reply.Sample.Helpers
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
