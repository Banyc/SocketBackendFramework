namespace SocketBackendFramework.Reply.Models.Middlewares
{
    // a preset pipe that has already hidden the reference to the next pipe from the parameters
    public delegate void MiddlewareRequestDelegate<TMiddlewareContext>(TMiddlewareContext context);
}
