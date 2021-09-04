using System;

namespace SocketBackendFramework.Middlewares.Codec
{
    public class Codec<TMiddlewareContext> : IMiddleware<TMiddlewareContext>
    {
        private readonly IHeaderCodec<TMiddlewareContext> headerCodec;

        public Codec(IHeaderCodec<TMiddlewareContext> headerCodec)
        {
            this.headerCodec = headerCodec;
        }

        public void Invoke(TMiddlewareContext context, Action next)
        {
            this.headerCodec.DecodeRequest(context);
            next();
            this.headerCodec.EncodeResponse(context);
        }
    }
}
