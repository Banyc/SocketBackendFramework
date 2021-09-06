using System.Collections.Generic;
using SocketBackendFramework.Models.Listeners;

namespace SocketBackendFramework.Models.Transport
{
    public class TransportMapperConfig
    {
        public List<ListenerConfig> ListenerConfigs { get; set; }
    }
}
