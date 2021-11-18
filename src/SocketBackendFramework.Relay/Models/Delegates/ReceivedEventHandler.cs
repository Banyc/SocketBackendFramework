using System.Net;

namespace SocketBackendFramework.Relay.Models.Delegates
{
    public delegate void ReceivedEventHandler(
        object sender,
        string transportType,
        EndPoint localEndPoint,
        EndPoint remoteEndpoint,
        byte[] buffer, long offset, long size);
}
