using SocketBackendFramework.Reply.Models;

namespace SocketBackendFramework.Reply.Middlewares.ContextAdaptor
{
    public interface IContextAdaptor<TMiddlewareContext>
    {
        TMiddlewareContext GetMiddlewareContext(PacketContext packetContext);
        PacketContext GetPacketContext(TMiddlewareContext middlewareContext);
    }
}
