namespace SocketBackendFramework.Relay.Models.Transport.PacketContexts
{
    public enum DownwardEventType
    {
        // ServerStarting,
        // TcpServerAccepting,

        TcpServerConnected,
        Disconnected,
        ApplicationMessageReceived,
    }

    public class DownwardPacketContext
    {
        public DownwardEventType EventType { get; set; }
        public FiveTuples? FiveTuples { get; set; }
        public uint TransportAgentId { get; set; }

        public byte[]? PacketRawBuffer { get; set; }
        public long PacketRawOffset { get; set; }
        public long PacketRawSize { get; set; }
    }
}
