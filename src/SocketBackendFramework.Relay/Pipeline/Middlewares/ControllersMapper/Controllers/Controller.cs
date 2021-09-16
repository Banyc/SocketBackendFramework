namespace SocketBackendFramework.Relay.Pipeline.Middlewares.ControllersMapper.Controllers
{
    public abstract class Controller<TMiddlewareContext>
    {
        public abstract bool IsControllerMatch(TMiddlewareContext context);
        // TODO: action route
        public abstract void Request(TMiddlewareContext context);
    }
}
