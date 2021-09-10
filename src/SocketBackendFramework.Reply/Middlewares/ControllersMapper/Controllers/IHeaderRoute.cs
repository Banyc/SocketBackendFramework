namespace SocketBackendFramework.Reply.Middlewares.ControllersMapper.Controllers
{
    public interface IHeaderRoute<TMiddlewareContext>
    {
	    bool IsThisContextMatchThisController(TMiddlewareContext context);
    }
}
