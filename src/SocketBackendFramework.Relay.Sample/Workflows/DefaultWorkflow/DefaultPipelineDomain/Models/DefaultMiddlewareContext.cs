using SocketBackendFramework.Relay.Models.Transport.PacketContexts;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models
{
    public class DefaultMiddlewareRequestContext
    {
        public DownwardPacketContext PacketContext { get; set; }
        public DefaultPacketHeader Header { get; set; } = new();
        public DefaultPacketBody Body { get; set; } = new();
    }

    public class DefaultMiddlewareResponseContext
    {
        public UpwardPacketContext PacketContext { get; set; } = new();
        public DefaultPacketHeader Header { get; set; } = new();
        public DefaultPacketBody Body { get; set; } = new();
    }

    public class DefaultMiddlewareContext
    {
        public DefaultMiddlewareRequestContext? Request { get; set; }
        public DefaultMiddlewareResponseContext Response { get; set; } = new();
    }
}
