using SocketBackendFramework.Relay.Pipeline;
using SocketBackendFramework.Relay.Pipeline.Middlewares.ControllersMapper.Controllers;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;

namespace SocketBackendFramework.Reply.Sample.Controllers
{
    public class EchoController : Controller<DefaultMiddlewareContext>
    {
        public class EchoControllerHeaderRoute : IHeaderRoute<DefaultMiddlewareContext>
        {
            public bool IsThisContextMatchThisController(DefaultMiddlewareContext context)
            {
                return context.RequestHeader.Type == DefaultPacketHeaderType.Echo ||
                       context.RequestHeader.Type == DefaultPacketHeaderType.EchoByClient;
            }
        }

        private readonly Pipeline<DefaultMiddlewareContext> defaultPipeline;

        public EchoController(Pipeline<DefaultMiddlewareContext> defaultPipeline)
        {
            this.defaultPipeline = defaultPipeline;
        }

        public override IHeaderRoute<DefaultMiddlewareContext> HeaderRoute => new EchoControllerHeaderRoute();

        public override void Request(DefaultMiddlewareContext context)
        {
            context.ResponseHeader = new()
            {
                Type = context.RequestHeader.Type,
            };
            context.ResponseBody = context.RequestBody;

            if (context.RequestHeader.Type == DefaultPacketHeaderType.EchoByClient)
            {
                context.PacketContext.ClientConfig = new()
                {
                    ClientDisposeTimeout = TimeSpan.FromSeconds(2),
                    RemoteAddress = context.PacketContext.RemoteIp.ToString(),
                    RemotePort = context.PacketContext.RemotePort,
                    TransportType = Relay.Models.Transport.ExclusiveTransportType.Udp,
                };
            }

            this.defaultPipeline.GoUp(context);
        }
    }
}
