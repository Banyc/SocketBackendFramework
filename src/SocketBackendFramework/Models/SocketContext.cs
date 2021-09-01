using System.Collections.Generic;
using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Models
{
    public class SocketContext
    {
        public string RemoteIp { get; set; }
        public int RemotePort { get; set; }
        public int LocalPort { get; set; }
        public List<byte> RequestPacketRaw { get; set; }
        public IRequestHeaderModel RequestHeader { get; set; }
        public IRequestBodyModel RequestBody { get; set; }
        public bool ShouldRespond { get; set; }
        public IResponseHeaderModel ResponseHeader { get; set; }
        public IResponseBodyModel ResponseBody { get; set; }
    }
}
