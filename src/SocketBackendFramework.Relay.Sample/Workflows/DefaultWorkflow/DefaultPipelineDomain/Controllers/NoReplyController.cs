using System;
using SocketBackendFramework.Relay.Pipeline.Middlewares.ControllersMapper.Controllers;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Controllers
{
    public class NoReplyController : Controller<DefaultMiddlewareContext>
    {
        public class NoReplyControllerHeaderRoute : IHeaderRoute<DefaultMiddlewareContext>
        {
            public bool IsThisContextMatchThisController(DefaultMiddlewareContext context)
            {
                return
                    context.PacketContext.PacketContextType ==
                        Relay.Models.PacketContextType.ApplicationMessage &&
                    context.RequestHeader.Type == DefaultPacketHeaderType.NoReply;
            }
        }

        public override IHeaderRoute<DefaultMiddlewareContext> HeaderRoute => new NoReplyControllerHeaderRoute();

        public override void Request(DefaultMiddlewareContext context)
        {
            Console.WriteLine($"[NoReplyController] received a packet with message: {context.RequestBody.Message}");
        }
    }
}
