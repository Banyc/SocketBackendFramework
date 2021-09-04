namespace SocketBackendFramework.Middlewares.ControllersMapper.Controllers
{
    public abstract class Controller<TMiddlewareContext>
    {
        public abstract IHeaderRoute<TMiddlewareContext> HeaderRoute { get; }
        // TODO: action route
        public abstract void Request(TMiddlewareContext context);
    }
}
