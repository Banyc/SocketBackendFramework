using System;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
using SocketBackendFramework.Relay.Pipeline.Middlewares.ControllersMapper.Controllers;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Controllers
{
    public class NoReplyController : Controller<DefaultMiddlewareContext>
    {
        public override bool IsControllerMatch(DefaultMiddlewareContext context)
        {
            return
                context.Request.PacketContext.EventType ==
                    DownwardEventType.ApplicationMessageReceived &&
                context.Request.Header.Type == DefaultPacketHeaderType.NoReply;
        }

        public override void Request(DefaultMiddlewareContext context)
        {
            Console.WriteLine($"[NoReplyController] received a packet with message: {context.Request.Body.Message}");
        }
    }
}
