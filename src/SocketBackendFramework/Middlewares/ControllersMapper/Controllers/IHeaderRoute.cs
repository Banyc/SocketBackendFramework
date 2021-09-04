using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Middlewares.ControllersMapper.Controllers
{
    public interface IHeaderRoute<TMiddlewareContext>
    {
	    bool IsThisContextMatchThisController(TMiddlewareContext context);
    }
}
