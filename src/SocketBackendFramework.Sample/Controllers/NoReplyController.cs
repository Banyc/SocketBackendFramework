using SocketBackendFramework.Middlewares.ControllersMapper.Controllers;
using SocketBackendFramework.Sample.Models;

namespace SocketBackendFramework.Sample.Controllers
{
    public class NoReplyController : Controller<MiddlewareContext>
    {
        public class NoReplyControllerHeaderRoute : IHeaderRoute<MiddlewareContext>
        {
            public bool IsThisContextMatchThisController(MiddlewareContext context)
            {
                return context.RequestHeader.Type == PacketType.NoReply;
            }
        }

        public override IHeaderRoute<MiddlewareContext> HeaderRoute => new NoReplyControllerHeaderRoute();

        public override void Request(MiddlewareContext context)
        {
            context.PacketContext.ShouldRespond = false;
        }
    }
}
