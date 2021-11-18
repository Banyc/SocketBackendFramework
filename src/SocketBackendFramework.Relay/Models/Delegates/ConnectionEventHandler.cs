using System.Net;

namespace SocketBackendFramework.Relay.Models.Delegates
{
    public delegate void ConnectionEventHandler(
        object sender,
        string transportType,
        EndPoint localEndPoint,
        EndPoint remoteEndPoint);
}
