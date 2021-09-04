using SocketBackendFramework.Models;
using SocketBackendFramework.Sample.Models.Middlewares;

namespace SocketBackendFramework.Sample.Models
{
    public class MiddlewareContext
    {
        public PacketContext PacketContext { get; set; }
        public PacketBody RequestBody { get; set; } = new();
        public PacketBody ResponseBody { get; set; } = new();
        public RequestHeader RequestHeader { get; set; } = new();
        public ResponseHeader ResponseHeader { get; set; } = new();
    }
}
