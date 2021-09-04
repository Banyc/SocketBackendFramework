using SocketBackendFramework.Middlewares.Codec;
using SocketBackendFramework.Sample.Models;

namespace SocketBackendFramework.Sample.Codec
{
    public class DefaultHeaderCodec : IHeaderCodec<MiddlewareContext>
    {
        public void DecodeRequest(MiddlewareContext context)
        {
        }

        public void EncodeResponse(MiddlewareContext context)
        {
        }
    }
}
