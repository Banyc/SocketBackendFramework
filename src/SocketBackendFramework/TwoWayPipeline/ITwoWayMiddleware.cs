namespace SocketBackendFramework.TwoWayPipeline
{
    public interface ITwoWayMiddleware<TMiddlewareContext>
    {
        void GoDown(TMiddlewareContext context);
        void GoUp(TMiddlewareContext context);
    }
}
