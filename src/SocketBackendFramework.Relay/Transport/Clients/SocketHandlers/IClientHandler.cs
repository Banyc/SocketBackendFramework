using System;
using System.Net;
using SocketBackendFramework.Relay.Models.Delegates;

namespace SocketBackendFramework.Relay.Transport.Clients.SocketHandlers
{
    public interface IClientHandler : IDisposable
    {
        event ConnectionEventHandler Connected;
        event ReceivedEventHandler Received;
        event ConnectionEventHandler Disconnected;
        void Connect();
        void Send(byte[] buffer, long offset, long size);
        void Disconnect();
        string TransportType { get; }
        EndPoint? LocalEndPoint { get; }
        EndPoint? RemoteEndPoint { get; }
    }
}
