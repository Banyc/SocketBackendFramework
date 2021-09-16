using System.Collections.Generic;
using SocketBackendFramework.Relay.Pipeline.Middlewares.ControllersMapper.Controllers;

namespace SocketBackendFramework.Relay.Pipeline.Middlewares.ControllersMapper
{
    public class ControllersMapper<TMiddlewareContext> : IMiddleware<TMiddlewareContext>
    {
        private readonly List<Controller<TMiddlewareContext>> controllers = new();

        public void AddController(Controller<TMiddlewareContext> controller)
        {
            this.controllers.Add(controller);
        }

        public void GoDown(TMiddlewareContext context)
        {
            foreach (Controller<TMiddlewareContext> controller in this.controllers)
            {
                if (controller.IsControllerMatch(context))
                {
                    controller.Request(context);
                    break;
                }
            }
        }

        public void GoUp(TMiddlewareContext context)
        {
        }
    }
}
