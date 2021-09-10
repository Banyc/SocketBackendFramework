using SocketBackendFramework.Relay.Pipeline;

namespace SocketBackendFramework.Relay.Pipeline.Middlewares.Codec
{
    public class Codec<TMiddlewareContext> : IMiddleware<TMiddlewareContext>
    {
        private readonly IHeaderCodec<TMiddlewareContext> headerCodec;

        public Codec(IHeaderCodec<TMiddlewareContext> headerCodec)
        {
            this.headerCodec = headerCodec;
        }

        public void GoDown(TMiddlewareContext context)
        {
            this.headerCodec.DecodeRequest(context);
        }

        public void GoUp(TMiddlewareContext context)
        {
            this.headerCodec.EncodeResponse(context);
        }
    }
}
