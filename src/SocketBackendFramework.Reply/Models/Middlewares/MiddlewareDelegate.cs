using System;

namespace SocketBackendFramework.Reply.Models.Middlewares
{
    public delegate void MiddlewareActionDelegate<TMiddlewareContext>(TMiddlewareContext context, Action next);
}
