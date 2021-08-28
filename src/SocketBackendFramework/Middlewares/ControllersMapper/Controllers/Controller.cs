using SocketBackendFramework.Models;

namespace SocketBackendFramework.Middlewares.ControllersMapper.Controllers
{
    public abstract class Controller
    {
        public IHeaderRoute HeaderRoute { get; set; }
        // TODO: action route
        public abstract void Request(SocketContext context);
    }
}
