using SocketBackendFramework.Models;
using SocketBackendFramework.Models.Middlewares;
using SocketBackendFramework.Sample.Models.Middlewares;

namespace SocketBackendFramework.Sample.Models
{
    public class MiddlewareContext : IMiddlewareContext
    {
        public PacketContext PacketContext { get; set; }
        public PacketBody? RequestBody { get; set; }
        public PacketBody? ResponseBody { get; set; }
        public RequestHeader? RequestHeader { get; set; }
        public ResponseHeader? ResponseHeader { get; set; }
    }
}
