using System;
using System.Collections.Generic;
using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Middlewares
{
    public class PipelineBuilder<TMiddlewareContext>
    {
        private readonly List<IMiddleware<TMiddlewareContext>> middlewares = new();
        // private List<MiddlewareDelegate> middlewareActions = new();
        private readonly List<Func<MiddlewareRequestDelegate<TMiddlewareContext>, MiddlewareRequestDelegate<TMiddlewareContext>>> pipeWrappers =
            new();

        public void UseMiddleware(IMiddleware<TMiddlewareContext> middleware)
        {
            this.middlewares.Add(middleware);
            Use((context, next) =>
            {
                middleware.Invoke(context, next);
            });
        }

        // stack the pipeWrappers
        public void Use(Func<MiddlewareRequestDelegate<TMiddlewareContext>, MiddlewareRequestDelegate<TMiddlewareContext>> pipeWrapper)
        {
            this.pipeWrappers.Add(pipeWrapper);
        }

        public void Use(MiddlewareActionDelegate<TMiddlewareContext> middlewareAction)
        {
            Func<MiddlewareRequestDelegate<TMiddlewareContext>, MiddlewareRequestDelegate<TMiddlewareContext>> pipeWrapper = next =>
            {
                // preset the `context` value to the `next` parameter in `middlewareAction`
                MiddlewareRequestDelegate<TMiddlewareContext> presetPipe = context =>
                {
                    // the object pointer (`context`) has been saved in `simpleNext`
                    Action simpleNext = () =>
                    {
                        // invoke the next preset pipe
                        next(context);
                    };

                    // execute the main middleware logic
                    middlewareAction(context, simpleNext);
                };
                return presetPipe;
            };
            Use(pipeWrapper);
        }

        public MiddlewareRequestDelegate<TMiddlewareContext> Build()
        {
            MiddlewareRequestDelegate<TMiddlewareContext> nextWrappedPine = null;

            int i;
            for (i = this.pipeWrappers.Count - 1; i >= 0; i--)
            {
                nextWrappedPine = this.pipeWrappers[i](nextWrappedPine);
            }

            return nextWrappedPine;
        }
    }
}
