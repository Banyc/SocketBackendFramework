using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SocketBackendFramework.Relay.Models.Delegates;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;

namespace SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers
{
    public interface IServerHandler : IDisposable
    {
        event ConnectionEventHandler ClientConnected;
        event ConnectionEventHandler ClientDisconnected;
        event ReceivedEventHandler ClientMessageReceived;

        void Start();
        void Send(EndPoint remoteEndPoint, byte[] buffer, long offset, long size);
        void Disconnect(EndPoint remoteEndPoint);
        string TransportType { get; }
        EndPoint LocalEndPoint { get; }
    }
}
