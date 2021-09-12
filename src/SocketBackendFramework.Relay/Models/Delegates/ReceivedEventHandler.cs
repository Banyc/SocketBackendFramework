using System.Net;

namespace SocketBackendFramework.Relay.Models.Delegates
{
    public delegate void ReceivedEventHandler(object sender, EndPoint remoteEndpoint, byte[] buffer, long offset, long size);
}
