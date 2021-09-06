using System.Collections.Generic;
using SocketBackendFramework.Relay.Models.Transport.Listeners;

namespace SocketBackendFramework.Relay.Models.Transport
{
    public class TransportMapperConfig
    {
        public List<ListenerConfig> ListenerConfigs { get; set; }
    }
}
