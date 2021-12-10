using System;
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
                ShouldSendSmallPacketsNoDelay = false,
                RetransmissionTimeout = TimeSpan.FromSeconds(3),
                OutputDuration = TimeSpan.FromMilliseconds(10),
            };
            return new KcpControl(config);
        }
    }
}
