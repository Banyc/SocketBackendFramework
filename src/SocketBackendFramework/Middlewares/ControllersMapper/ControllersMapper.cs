using System;
using System.Collections.Generic;
using SocketBackendFramework.Middlewares.ControllersMapper.Controllers;
using SocketBackendFramework.Models;

namespace SocketBackendFramework.Middlewares.ControllersMapper
{
    public class ControllersMapper : IMiddleware
    {
        private readonly List<Controller> controllers = new();

        public void AddController(Controller controller)
        {
            this.controllers.Add(controller);
        }

        public void Invoke(PacketContext context, Action next)
        {
            foreach (Controller controller in this.controllers)
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
