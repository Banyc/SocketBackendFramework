using System.Net;

namespace SocketBackendFramework.Relay.Transport.Clients.SocketHandlers
{
    public interface IClientHandlerBuilder
    {
        IClientHandler Build(IPEndPoint remoteEndPoint, object? config);
    }
}
