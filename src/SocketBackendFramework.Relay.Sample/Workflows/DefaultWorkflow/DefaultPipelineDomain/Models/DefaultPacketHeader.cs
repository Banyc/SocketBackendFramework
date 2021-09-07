namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models
{
    public enum DefaultPacketHeaderType
    {
        NoReply,
        Echo,
        Forward,
    }

    public class DefaultPacketHeader
    {
        public DefaultPacketHeaderType Type { get; set; }
    }
}
