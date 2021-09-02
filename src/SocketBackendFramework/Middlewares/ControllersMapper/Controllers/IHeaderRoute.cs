using SocketBackendFramework.Models;

namespace SocketBackendFramework.Middlewares.ControllersMapper.Controllers
{
    public interface IHeaderRoute
    {
	    bool IsThisContextMatchThisController(PacketContext context);
    }
}
