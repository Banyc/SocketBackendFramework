using System;
using System.Collections.Generic;
using SocketBackendFramework.Reply.Middlewares.ControllersMapper.Controllers;

namespace SocketBackendFramework.Reply.Middlewares.ControllersMapper
{
    public class ControllersMapper<TMiddlewareContext> : IMiddleware<TMiddlewareContext>
    {
        private readonly List<Controller<TMiddlewareContext>> controllers = new();

        public void AddController(Controller<TMiddlewareContext> controller)
        {
            this.controllers.Add(controller);
        }

        public void Invoke(TMiddlewareContext context, Action next)
        {
            foreach (Controller<TMiddlewareContext> controller in this.controllers)
            {
                if (controller.HeaderRoute.IsThisContextMatchThisController(context))
                {
                    controller.Request(context);
                    break;
                }
            }
        }
    }
}
