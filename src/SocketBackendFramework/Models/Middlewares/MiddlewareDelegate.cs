using System;

namespace SocketBackendFramework.Models.Middlewares
{
    public delegate void MiddlewareActionDelegate(SocketContext context, Action next);
}
