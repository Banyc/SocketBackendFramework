namespace SocketBackendFramework.Reply.Models.Listeners
{
    public enum ExclusiveTransportType
    {
        Tcp,
        Udp,
    }

    public class ListenerConfig
    {
        public int ListeningPort { get; set; }
        public ExclusiveTransportType TransportType { get; set; }
    }
}
