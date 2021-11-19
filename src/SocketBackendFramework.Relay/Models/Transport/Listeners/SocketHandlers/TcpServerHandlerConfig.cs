using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocketBackendFramework.Relay.Models.Transport.Listeners.SocketHandlers
{
    public class TcpServerHandlerBuilderConfig
    {
        // ConfigId -> TcpServerHandlerConfig
        public Dictionary<string, TcpServerHandlerConfig> TcpServerHandlers { get; set; }
    }
    public class TcpServerHandlerConfig
    {
        public double SessionTimeoutMs { get; set; }
    }
}
