using System;
using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Middlewares
{
    public interface IMiddleware
    {
        // MiddlewareActionDelegate Invoke { get; }
        void Invoke(IMiddlewareContext context, Action next);
    }
}
