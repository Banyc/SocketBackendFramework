using System.Net;

namespace SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers
{
    public interface IServerHandlerBuilder
    {
        IServerHandler Build(IPAddress ipAddress, int port, string? configId);
    }
}
