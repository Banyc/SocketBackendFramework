namespace SocketBackendFramework.Relay.Pipeline
{
    public class PipelineBuilder<TMiddlewareContext>
    {
        private List<IMiddleware<TMiddlewareContext>> middlewares = new();
        public void Use(IMiddleware<TMiddlewareContext> middleware)
        {
            this.middlewares.Add(middleware);
        }
        public Pipeline<TMiddlewareContext> Build()
        {
            Pipeline<TMiddlewareContext> pipeline = new(this.middlewares);
            this.middlewares = new();
            return pipeline;
        }
    }
}
