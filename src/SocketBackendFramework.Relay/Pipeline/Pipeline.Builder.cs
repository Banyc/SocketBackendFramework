namespace SocketBackendFramework.Relay.Pipeline
{
    public partial class Pipeline<TMiddlewareContext>
    {
        public Pipeline()
        {
        }

        public void Use(IMiddleware<TMiddlewareContext> middleware)
        {
            this.middlewares.Add(middleware);
        }
    }
}
