using SocketBackendFramework.Relay.Models.Transport.PacketContexts;

namespace SocketBackendFramework.Relay.ContextAdaptor
{
    public interface IContextAdaptor<TMiddlewareContext>
    {
        TMiddlewareContext GetMiddlewareContext(DownwardPacketContext packetContext);
        UpwardPacketContext GetPacketContext(TMiddlewareContext middlewareContext);
    }
}
