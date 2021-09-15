using SocketBackendFramework.Relay.Pipeline;
using SocketBackendFramework.Relay.Pipeline.Middlewares.ControllersMapper.Controllers;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Controllers
{
    public class GreetController : Controller<DefaultMiddlewareContext>
    {
        private readonly Pipeline<DefaultMiddlewareContext> defaultPipeline;

        public class GreetControllerHeaderRoute : IHeaderRoute<DefaultMiddlewareContext>
        {
            public bool IsThisContextMatchThisController(DefaultMiddlewareContext context)
            {
                return
                    context.PacketContext.PacketContextType ==
                        Relay.Models.PacketContextType.TcpServerConnection;
            }
        }

        public override IHeaderRoute<DefaultMiddlewareContext> HeaderRoute => new GreetControllerHeaderRoute();

        public GreetController(Pipeline<DefaultMiddlewareContext> defaultPipeline)
        {
            this.defaultPipeline = defaultPipeline;
        }

        public override void Request(DefaultMiddlewareContext context)
        {
            context.PacketContext.PacketContextType = Relay.Models.PacketContextType.ApplicationMessage;
            context.ResponseHeader = new()
            {
                Type = DefaultPacketHeaderType.NoReply,
            };
            context.ResponseBody = new()
            {
                Message = "The server is ready.",
            };
            this.defaultPipeline.GoUp(context);
        }
    }
}
