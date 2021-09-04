using System.Collections.Generic;
using SocketBackendFramework.Models;
using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Middlewares.Codec
{
    public interface IHeaderCodec
    {
        void DecodeRequest(IMiddlewareContext context);
        void EncodeResponse(IMiddlewareContext context);
    }
}
