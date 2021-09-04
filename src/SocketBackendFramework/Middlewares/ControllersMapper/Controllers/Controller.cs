using SocketBackendFramework.Models;
using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Middlewares.ControllersMapper.Controllers
{
    public abstract class Controller<TMiddlewareContext>
    {
        public IHeaderRoute<TMiddlewareContext> HeaderRoute { get; set; }
        // TODO: action route
        public abstract void Request(TMiddlewareContext context);
    }
}
