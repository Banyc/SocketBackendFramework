using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.DefaultSocketHandlers
{
    public class KcpControlBuilder
    {
        public KcpControl Build()
        {
            KcpConfig config = new()
            {
                ConversationId = 0x0,
                IsStreamMode = false,
                ReceiveWindowSize = 15,
                IsNoDelayAck = false,
            };
            return new KcpControl(config);
        }
    }
}
