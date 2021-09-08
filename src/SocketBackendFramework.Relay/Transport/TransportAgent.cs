using SocketBackendFramework.Relay.Models;

namespace SocketBackendFramework.Relay.Transport
{
    public interface ITransportAgent
    {
        event EventHandler<PacketContext> PacketReceived;

        void Respond(PacketContext context);
    }
}
