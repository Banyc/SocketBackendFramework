using System;
namespace SocketBackendFramework.Reply.Middlewares
{
    public interface IMiddleware<TMiddlewareContext>
    {
        // MiddlewareActionDelegate Invoke { get; }
        void Invoke(TMiddlewareContext context, Action next);
    }
}
