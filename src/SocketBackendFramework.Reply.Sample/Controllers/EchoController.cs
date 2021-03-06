using SocketBackendFramework.Reply.Middlewares.ControllersMapper.Controllers;
using SocketBackendFramework.Reply.Sample.Models;

namespace SocketBackendFramework.Reply.Sample.Controllers
{
    public class EchoController : Controller<MiddlewareContext>
    {
        public class EchoControllerHeaderRoute : IHeaderRoute<MiddlewareContext>
        {
            public bool IsThisContextMatchThisController(MiddlewareContext context)
            {
                return context.RequestHeader.Type == PacketType.Echo;
            }
        }
        public override IHeaderRoute<MiddlewareContext> HeaderRoute => new EchoControllerHeaderRoute();

        public override void Request(MiddlewareContext context)
        {
            context.ResponseHeader = new()
            {
                Type = context.RequestHeader.Type,
            };
            context.ResponseBody = context.RequestBody;

            context.PacketContext.ShouldRespond = true;
        }
    }
}
