using System;
using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Middlewares
{
    public interface IMiddleware<TMiddlewareContext>
    {
        // MiddlewareActionDelegate Invoke { get; }
        void Invoke(TMiddlewareContext context, Action next);
    }
}
