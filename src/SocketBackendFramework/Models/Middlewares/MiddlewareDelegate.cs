using System;

namespace SocketBackendFramework.Models.Middlewares
{
    public delegate void MiddlewareActionDelegate<TMiddlewareContext>(TMiddlewareContext context, Action next);
}
