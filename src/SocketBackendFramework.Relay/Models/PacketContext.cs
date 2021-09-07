using System.Net;

namespace SocketBackendFramework.Relay.Models
{
    public class PacketContext
    {
        public IPAddress? RemoteIp { get; set; }
        public int RemotePort { get; set; }

        // if null, create a dedicated socket client
        public int? LocalPort { get; set; }

        public byte[]? RequestPacketRawBuffer { get; set; }
        public long RequestPacketRawOffset { get; set; }
        public long RequestPacketRawSize { get; set; }
        public bool ShouldRespond { get; set; }
        public List<byte> ResponsePacketRaw { get; set; } = new();

        // necessary if this.LocalPort is null 
        public TimeSpan ClientDisposeTimeout { get; set; }
    }
}
