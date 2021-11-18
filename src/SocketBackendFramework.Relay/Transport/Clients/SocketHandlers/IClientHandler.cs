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
        event ConnectionEventArgs Connected;
        event ReceivedEventArgs Received;
        event ConnectionEventArgs Disconnected;
        void Connect();
        void Send(byte[] buffer, long offset, long size);
        void Disconnect();
        EndPoint LocalEndPoint { get; }
        EndPoint RemoteEndPoint { get; }
        string TransportType { get; }
    }
}
