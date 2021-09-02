using System;

namespace SocketBackendFramework.Models.Middlewares
{
    public delegate void MiddlewareActionDelegate(PacketContext context, Action next);
}
