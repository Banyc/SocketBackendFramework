namespace SocketBackendFramework.Relay.Pipeline.Middlewares.ControllersMapper.Controllers
{
    public interface IHeaderRoute<TMiddlewareContext>
    {
	    bool IsThisContextMatchThisController(TMiddlewareContext context);
    }
}
