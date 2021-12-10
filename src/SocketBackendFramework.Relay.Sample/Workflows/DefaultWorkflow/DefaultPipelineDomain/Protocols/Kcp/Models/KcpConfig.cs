using System;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models
{
    public class KcpConfig
    {
        public uint ConversationId { get; set; }
        public bool IsStreamMode { get; set; }
        public uint ReceiveWindowSize { get; set; }
        public bool ShouldSendSmallPacketsNoDelay { get; set; }
        public TimeSpan? RetransmissionTimeout { get; set; } = null;
        public TimeSpan? OutputDuration { get; set; } = null;
    }
}
