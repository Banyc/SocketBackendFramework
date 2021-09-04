using SocketBackendFramework.Models;
using SocketBackendFramework.Sample.Models.Middlewares;

namespace SocketBackendFramework.Sample.Models
{
    public class MiddlewareContext
    {
        public PacketContext PacketContext { get; set; }
        public PacketBody? RequestBody { get; set; }
        public PacketBody? ResponseBody { get; set; }
        public RequestHeader? RequestHeader { get; set; }
        public ResponseHeader? ResponseHeader { get; set; }
    }
}
