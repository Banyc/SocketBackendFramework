using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SocketBackendFramework.Relay.Models.Delegates;

namespace SocketBackendFramework.Relay.Transport.Clients.SocketHandlers
{
    public interface IClientHandler : IDisposable
    {
        event SimpleEventHandler Connected;
        event ReceivedEventHandler Received;
        event SimpleEventHandler Disconnected;
        void Connect();
        void Send(byte[] buffer, long offset, long size);
        void Disconnect();
        EndPoint GetLocalEndPoint();
    }
}
