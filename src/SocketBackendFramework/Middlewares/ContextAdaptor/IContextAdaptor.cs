using SocketBackendFramework.Models;
using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Middlewares.ContextAdaptor
{
    public interface IContextAdaptor
    {
        IMiddlewareContext GetMiddlewareContext(PacketContext packetContext);
        PacketContext GetPacketContext(IMiddlewareContext middlewareContext);
    }
}
