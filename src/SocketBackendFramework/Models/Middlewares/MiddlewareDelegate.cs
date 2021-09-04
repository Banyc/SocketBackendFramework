using System;

namespace SocketBackendFramework.Models.Middlewares
{
    public delegate void MiddlewareActionDelegate(IMiddlewareContext context, Action next);
}
