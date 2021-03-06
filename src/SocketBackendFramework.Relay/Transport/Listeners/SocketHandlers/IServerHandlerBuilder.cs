using System.Net;

namespace SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers
{
    public interface IServerHandlerBuilder
    {
        IServerHandler Build(IPEndPoint localEndPoint, string? configId);
    }
}
