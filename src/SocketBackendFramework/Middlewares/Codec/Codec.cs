using System;
using SocketBackendFramework.Models;

namespace SocketBackendFramework.Middlewares.Codec
{
    public class Codec : IMiddleware
    {
        private readonly IHeaderCodec headerCodec;

        public Codec(IHeaderCodec headerCodec)
        {
            this.headerCodec = headerCodec;
        }

        public void Invoke(PacketContext context, Action next)
        {
            this.headerCodec.DecodeRequest(context);
            next();
            this.headerCodec.EncodeResponse(context);
        }
    }
}
