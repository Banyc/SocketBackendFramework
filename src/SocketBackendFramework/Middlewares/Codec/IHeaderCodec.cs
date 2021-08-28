using System.Collections.Generic;
using SocketBackendFramework.Models;

namespace SocketBackendFramework.Middlewares.Codec
{
    public interface IHeaderCodec
    {
        // SocketContext GetSocketContext(List<byte> packet);
        // List<byte> GetResponseBytes(SocketContext context);
        void DecodeRequest(SocketContext context);
        void EncodeResponse(SocketContext context);
    }
}
