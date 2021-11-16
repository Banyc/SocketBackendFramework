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

        public byte[]? PacketRawBuffer { get; set; }
        public long PacketRawOffset { get; set; }
        public long PacketRawSize { get; set; }

        // if not null, create a dedicated socket client
        public TransportClientConfig? ClientConfig { get; set; }
    }
}
