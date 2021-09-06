namespace SocketBackendFramework.Relay.Pipeline
{
    public interface IMiddleware<TMiddlewareContext>
    {
        void GoDown(TMiddlewareContext context);
        void GoUp(TMiddlewareContext context);
    }
}
