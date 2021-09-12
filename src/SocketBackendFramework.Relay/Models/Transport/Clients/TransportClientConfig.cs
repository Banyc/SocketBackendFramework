namespace SocketBackendFramework.Relay.Models.Transport.Clients
{
    public class TransportClientConfig
    {
        public ExclusiveTransportType TransportType { get; set; }
        public string RemoteAddress { get; set; }
        public int RemotePort { get; set; }
        public TimeSpan ClientDisposeTimeout { get; set; }
    }
}
