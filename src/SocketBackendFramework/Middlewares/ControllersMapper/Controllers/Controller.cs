using SocketBackendFramework.Models;
using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Middlewares.ControllersMapper.Controllers
{
    public abstract class Controller
    {
        public IHeaderRoute HeaderRoute { get; set; }
        // TODO: action route
        public abstract void Request(IMiddlewareContext context);
    }
}
