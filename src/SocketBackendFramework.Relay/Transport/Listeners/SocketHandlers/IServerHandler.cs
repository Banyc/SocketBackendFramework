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
        event EventHandler<ConnectionEventArgs> ClientConnected;
        event EventHandler<ConnectionEventArgs> ClientDisconnected;
        event EventHandler<ReceivedEventArgs> ClientMessageReceived;

        void Start();
        void Disconnect(EndPoint remoteEndPoint);
        string TransportType { get; }
        EndPoint LocalEndPoint { get; }
    }
}
