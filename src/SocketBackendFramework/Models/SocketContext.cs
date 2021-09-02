using System.Collections.Generic;
using System.Net;
using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Models
{
    public class PacketContext
    {
        public IPAddress RemoteIp { get; set; }
        public int RemotePort { get; set; }
        public int LocalPort { get; set; }
        public byte[] RequestPacketRawBuffer { get; set; }
        public long RequestPacketRawOffset { get; set; }
        public long RequestPacketRawSize { get; set; }
        public IRequestHeaderModel RequestHeader { get; set; }
        public IRequestBodyModel RequestBody { get; set; }
        public bool ShouldRespond { get; set; }
        public List<byte> ResponsePacketRaw { get; set; }
        public IResponseHeaderModel ResponseHeader { get; set; }
        public IResponseBodyModel ResponseBody { get; set; }
    }
}
