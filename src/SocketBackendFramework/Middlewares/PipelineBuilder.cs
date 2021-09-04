using System;
using System.Collections.Generic;
using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Middlewares
{
    public class PipelineBuilder
    {
        private readonly List<IMiddleware> middlewares = new();
        // private List<MiddlewareDelegate> middlewareActions = new();
        private readonly List<Func<SocketRequestDelegate, SocketRequestDelegate>> pipeWrappers =
            new();

        public void UseMiddleware(IMiddleware middleware)
        {
            this.middlewares.Add(middleware);
            Use((context, next) =>
            {
                middleware.Invoke(context, next);
            });
        }

        // stack the pipeWrappers
        public void Use(Func<SocketRequestDelegate, SocketRequestDelegate> pipeWrapper)
        {
            this.pipeWrappers.Add(pipeWrapper);
        }

        public void Use(MiddlewareActionDelegate middlewareAction)
        {
            Func<SocketRequestDelegate, SocketRequestDelegate> pipeWrapper = next =>
            {
                // preset the `context` value to the `next` parameter in `middlewareAction`
                SocketRequestDelegate presetPipe = context =>
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

        public SocketRequestDelegate Build()
        {
            SocketRequestDelegate nextWrappedPine = null;

            int i;
            for (i = this.pipeWrappers.Count - 1; i >= 0; i--)
            {
                nextWrappedPine = this.pipeWrappers[i](nextWrappedPine);
            }

            return nextWrappedPine;
        }
    }
}
