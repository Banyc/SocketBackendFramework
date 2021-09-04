namespace SocketBackendFramework.Middlewares.Codec
{
    public interface IHeaderCodec<TMiddlewareContext>
    {
        void DecodeRequest(TMiddlewareContext context);
        void EncodeResponse(TMiddlewareContext context);
    }
}
