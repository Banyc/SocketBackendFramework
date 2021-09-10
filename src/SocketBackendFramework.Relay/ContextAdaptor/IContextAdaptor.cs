using SocketBackendFramework.Relay.Models;

namespace SocketBackendFramework.Relay.ContextAdaptor
{
    public interface IContextAdaptor<TMiddlewareContext>
    {
        TMiddlewareContext GetMiddlewareContext(PacketContext packetContext);
        PacketContext GetPacketContext(TMiddlewareContext middlewareContext);
    }
}
