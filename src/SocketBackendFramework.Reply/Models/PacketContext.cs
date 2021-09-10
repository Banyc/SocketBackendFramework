using System.Collections.Generic;
using System.Net;

namespace SocketBackendFramework.Reply.Models
{
    public class PacketContext
    {
        public IPAddress RemoteIp { get; set; }
        public int RemotePort { get; set; }
        public int LocalPort { get; set; }
        public byte[] RequestPacketRawBuffer { get; set; }
        public long RequestPacketRawOffset { get; set; }
        public long RequestPacketRawSize { get; set; }
        public bool ShouldRespond { get; set; }
        public List<byte> ResponsePacketRaw { get; set; } = new();
    }
}
