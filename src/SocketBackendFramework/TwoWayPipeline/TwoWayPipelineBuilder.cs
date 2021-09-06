using System.Collections.Generic;

namespace SocketBackendFramework.TwoWayPipeline
{
    public class TwoWayPipelineBuilder<TMiddlewareContext>
    {
        private List<ITwoWayMiddleware<TMiddlewareContext>> middlewares = new();
        public void Use(ITwoWayMiddleware<TMiddlewareContext> middleware)
        {
            this.middlewares.Add(middleware);
        }
        public TwoWayPipeline<TMiddlewareContext> Build()
        {
            TwoWayPipeline<TMiddlewareContext> pipeline = new(this.middlewares);
            this.middlewares = new();
            return pipeline;
        }
    }
}
