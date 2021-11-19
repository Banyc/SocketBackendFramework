using System;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;

namespace SocketBackendFramework.Relay.Models.Transport.Clients
{
    public delegate IClientHandler ClientHandlerBuilder(string type);

    public class TransportClientConfig
    {
        public string TransportType { get; set; }
        public string RemoteAddress { get; set; }
        public int RemotePort { get; set; }
        public TimeSpan ClientDisposeTimeout { get; set; }
        public string SocketHandlerConfigId { get; set; }
    }
}
