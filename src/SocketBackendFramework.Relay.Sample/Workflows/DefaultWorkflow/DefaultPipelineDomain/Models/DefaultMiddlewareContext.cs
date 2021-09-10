using SocketBackendFramework.Relay.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models
{
    public class DefaultMiddlewareContext
    {
        public PacketContext PacketContext { get; set; } = new();
        public DefaultPacketBody RequestBody { get; set; } = new();
        public DefaultPacketBody ResponseBody { get; set; } = new();
        public DefaultPacketHeader RequestHeader { get; set; } = new();
        public DefaultPacketHeader ResponseHeader { get; set; } = new();
    }
}
