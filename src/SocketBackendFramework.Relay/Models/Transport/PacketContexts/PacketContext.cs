namespace SocketBackendFramework.Relay.Models.Transport.PacketContexts
{
    public class PacketContext
    {
        public UpwardPacketContext? UpwardPacketContext { get; set; }
        public DownwardPacketContext? DownwardPacketContext { get; set; }
    }
}
