using System;
using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Middlewares.Codec
{
    public class Codec : IMiddleware
    {
        private readonly IHeaderCodec headerCodec;

        public Codec(IHeaderCodec headerCodec)
        {
            this.headerCodec = headerCodec;
        }

        public void Invoke(IMiddlewareContext context, Action next)
        {
            this.headerCodec.DecodeRequest(context);
            next();
            this.headerCodec.EncodeResponse(context);
        }
    }
}
