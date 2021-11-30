using System;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models
{
    public class KcpConfig
    {
        public uint ConversationId { get; set; }
        public bool IsStreamMode { get; set; }
        public uint ReceiveWindowSize { get; set; }
        public bool IsNoDelayAck { get; set; }
        public TimeSpan? RetransmissionTimeout { get; set; } = null;
        public TimeSpan? OutputDuration { get; set; } = null;
        public Action<byte[], int>? OutputCallback { get; set; } = null;
    }
}
