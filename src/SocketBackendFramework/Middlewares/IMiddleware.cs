using System;
using SocketBackendFramework.Models;

namespace SocketBackendFramework.Middlewares
{
    public interface IMiddleware
    {
        // MiddlewareActionDelegate Invoke { get; }
        void Invoke(PacketContext context, Action next);
    }
}
