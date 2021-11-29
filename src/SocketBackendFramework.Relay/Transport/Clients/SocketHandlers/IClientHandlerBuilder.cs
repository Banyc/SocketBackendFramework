namespace SocketBackendFramework.Relay.Transport.Clients.SocketHandlers
{
    public interface IClientHandlerBuilder
    {
        IClientHandler Build(string ipAddress, int port, string? configId);
    }
}
