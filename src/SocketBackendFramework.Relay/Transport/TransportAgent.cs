using SocketBackendFramework.Relay.Models;

namespace SocketBackendFramework.Relay.Transport
{
    public interface ITransportAgent
    {
        event EventHandler<PacketContext> PacketReceived;
        uint TransportAgentId { get; }

        void Respond(PacketContext context);
    }
}
