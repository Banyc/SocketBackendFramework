using System.Collections.Generic;
using SocketBackendFramework.Relay.Models.Transport.Clients;

namespace SocketBackendFramework.Relay.Models.Transport.PacketContexts
{
    public enum UpwardActionType
    {
        Disconnect,
        SendApplicationMessage,
    }

    public class UpwardPacketContext
    {
        public UpwardActionType ActionType { get; set; }
        public FiveTuples FiveTuples { get; set; }

        public List<byte> ResponsePacketRaw { get; set; } = new();

        // if not null, create a dedicated socket client
        public TransportClientConfig? ClientConfig { get; set; }
    }
}
