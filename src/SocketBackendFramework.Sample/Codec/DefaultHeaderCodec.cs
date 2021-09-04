using SocketBackendFramework.Middlewares.Codec;
using SocketBackendFramework.Models.Middlewares;
using SocketBackendFramework.Sample.Models;

namespace SocketBackendFramework.Sample.Codec
{
    public class DefaultHeaderCodec : IHeaderCodec
    {
        public void DecodeRequest(IMiddlewareContext context)
        {
            MiddlewareContext contextInstance = (MiddlewareContext)context;
        }

        public void EncodeResponse(IMiddlewareContext context)
        {
            MiddlewareContext contextInstance = (MiddlewareContext)context;
        }
    }
}
