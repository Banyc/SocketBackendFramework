using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SocketBackendFramework.Relay.Models.Transport.Listeners;

namespace SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers
{
    public interface IServerHandlerBuilder
    {
        IServerHandler Build(IPAddress ipAddress, int port, string configId);
    }
}
