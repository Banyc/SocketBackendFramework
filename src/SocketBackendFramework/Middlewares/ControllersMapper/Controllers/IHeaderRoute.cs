using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Middlewares.ControllersMapper.Controllers
{
    public interface IHeaderRoute
    {
	    bool IsThisContextMatchThisController(IMiddlewareContext context);
    }
}
