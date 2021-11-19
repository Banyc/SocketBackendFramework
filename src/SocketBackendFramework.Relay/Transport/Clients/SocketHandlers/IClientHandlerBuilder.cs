using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;

namespace SocketBackendFramework.Relay.Transport.Clients.SocketHandlers
{
    public interface IClientHandlerBuilder
    {
        IClientHandler Build(string ipAddress, int port, string configId);
    }
}
