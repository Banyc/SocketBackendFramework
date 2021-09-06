using System;
using System.Collections.Generic;
namespace SocketBackendFramework.TwoWayPipeline
{
    public class TwoWayPipeline<TMiddlewareContext> : ITwoWayMiddleware<TMiddlewareContext>
    {
        public event EventHandler<TMiddlewareContext> GoneDown;
        public event EventHandler<TMiddlewareContext> GoneUp;

        private readonly List<ITwoWayMiddleware<TMiddlewareContext>> middlewares;

        public TwoWayPipeline(List<ITwoWayMiddleware<TMiddlewareContext>> middlewares)
        {
            this.middlewares = middlewares;
        }

        public void GoDown(TMiddlewareContext context)
        {
            int i;
            for (i = 0; i < this.middlewares.Count; i++)
            {
                this.middlewares[i].GoDown(context);
            }
            this.GoneDown?.Invoke(this, context);
        }

        public void GoUp(TMiddlewareContext context)
        {
            int i;
            for (i = this.middlewares.Count - 1; i >= 0; i--)
            {
                this.middlewares[i].GoUp(context);
            }
            this.GoneUp?.Invoke(this, context);
        }
    }
}
