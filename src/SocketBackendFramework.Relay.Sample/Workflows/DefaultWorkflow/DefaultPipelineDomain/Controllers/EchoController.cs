using System;
using SocketBackendFramework.Relay.Models.Transport;
using SocketBackendFramework.Relay.Pipeline;
using SocketBackendFramework.Relay.Pipeline.Middlewares.ControllersMapper.Controllers;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Controllers
{
    public class EchoController : Controller<DefaultMiddlewareContext>
    {
        public class EchoControllerHeaderRoute : IHeaderRoute<DefaultMiddlewareContext>
        {
            public bool IsThisContextMatchThisController(DefaultMiddlewareContext context)
            {
                return
                    context.PacketContext.PacketContextType ==
                        Relay.Models.PacketContextType.ApplicationMessage &&
                    (
                        context.RequestHeader.Type == DefaultPacketHeaderType.Echo ||
                        context.RequestHeader.Type == DefaultPacketHeaderType.EchoByClient
                    );
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
            if (context.PacketContext.PacketContextType != Relay.Models.PacketContextType.ApplicationMessage)
            {
                return;
            }
            context.ResponseHeader = new()
            {
                Type = context.RequestHeader.Type,
            };
            context.ResponseBody = context.RequestBody;

            if (context.RequestHeader.Type == DefaultPacketHeaderType.EchoByClient)
            {
                int remotePort;
                if (context.PacketContext.TransportType == ExclusiveTransportType.Udp)
                {
                    remotePort = context.PacketContext.RemotePort;
                }
                else
                {
                    remotePort = 8082;
                }
                context.PacketContext.ClientConfig = new()
                {
                    ClientDisposeTimeout = TimeSpan.FromSeconds(2),
                    RemoteAddress = context.PacketContext.RemoteIp.ToString(),
                    RemotePort = remotePort,
                    TransportType = context.PacketContext.TransportType,
                };
            }

            this.defaultPipeline.GoUp(context);
        }
    }
}
