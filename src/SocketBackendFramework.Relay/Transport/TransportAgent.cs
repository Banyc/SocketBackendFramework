using System;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;

namespace SocketBackendFramework.Relay.Transport
{
    public interface ITransportAgent
    {
        event EventHandler<DownwardPacketContext> PacketReceived;
        uint TransportAgentId { get; }

        void Respond(UpwardPacketContext context);
    }
}
