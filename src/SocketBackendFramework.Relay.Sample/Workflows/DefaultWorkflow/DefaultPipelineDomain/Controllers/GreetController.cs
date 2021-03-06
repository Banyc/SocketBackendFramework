using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
using SocketBackendFramework.Relay.Pipeline;
using SocketBackendFramework.Relay.Pipeline.Middlewares.ControllersMapper.Controllers;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Controllers
{
    public class GreetController : Controller<DefaultMiddlewareContext>
    {
        private readonly Pipeline<DefaultMiddlewareContext> defaultPipeline;

        public GreetController(Pipeline<DefaultMiddlewareContext> defaultPipeline)
        {
            this.defaultPipeline = defaultPipeline;
        }

        public override bool IsControllerMatch(DefaultMiddlewareContext context)
        {
            return
                context.Request!.PacketContext!.EventType ==
                    DownwardEventType.TcpServerConnected;
        }

        public override void Request(DefaultMiddlewareContext context)
        {
            context.Response = new();
            context.Response.PacketContext.ActionType = UpwardActionType.SendApplicationMessage;
            context.Response.PacketContext.FiveTuples = context.Request!.PacketContext!.FiveTuples;
            context.Response.Header = new()
            {
                Type = DefaultPacketHeaderType.NoReply,
            };
            context.Response.Body = new()
            {
                Message = "The server is ready.",
            };
            this.defaultPipeline.GoUp(context);
        }
    }
}
