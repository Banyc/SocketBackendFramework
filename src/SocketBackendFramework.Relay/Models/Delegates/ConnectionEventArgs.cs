using System.Net;

namespace SocketBackendFramework.Relay.Models.Delegates
{
    public delegate void ConnectionEventArgs(
        object sender,
        string transportType,
        EndPoint localEndPoint,
        EndPoint remoteEndPoint);
}
