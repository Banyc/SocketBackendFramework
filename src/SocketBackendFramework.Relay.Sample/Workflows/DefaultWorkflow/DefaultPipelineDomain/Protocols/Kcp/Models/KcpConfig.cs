using System;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models
{
    public class KcpConfig : ICloneable
    {
        public uint ConversationId { get; set; }
        public bool IsStreamMode { get; set; }
        public uint ReceiveWindowSize { get; set; }
        public bool ShouldSendSmallPacketsNoDelay { get; set; }
        public TimeSpan? RetransmissionTimeout { get; set; } = null;
        public TimeSpan? OutputDuration { get; set; } = null;

        public object Clone()
        {
            return new KcpConfig()
            {
                ConversationId = this.ConversationId,
                IsStreamMode = this.IsStreamMode,
                ReceiveWindowSize = this.ReceiveWindowSize,
                ShouldSendSmallPacketsNoDelay = this.ShouldSendSmallPacketsNoDelay,
                RetransmissionTimeout = this.RetransmissionTimeout,
                OutputDuration = this.OutputDuration
            };
        }
    }
}
