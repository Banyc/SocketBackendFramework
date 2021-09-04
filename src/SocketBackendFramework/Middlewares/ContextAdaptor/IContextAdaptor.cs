using SocketBackendFramework.Models;

namespace SocketBackendFramework.Middlewares.ContextAdaptor
{
    public interface IContextAdaptor<TMiddlewareContext>
    {
        TMiddlewareContext GetMiddlewareContext(PacketContext packetContext);
        PacketContext GetPacketContext(TMiddlewareContext middlewareContext);
    }
}
