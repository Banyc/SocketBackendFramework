using System;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
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
                    context.Request.PacketContext.EventType ==
                        DownwardEventType.ApplicationMessageReceived &&
                    (
                        context.Request.Header.Type == DefaultPacketHeaderType.Echo ||
                        context.Request.Header.Type == DefaultPacketHeaderType.EchoByClient
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
            context.Response.PacketContext.ActionType = UpwardActionType.SendApplicationMessage;
            context.Response.PacketContext.FiveTuples = context.Request.PacketContext.FiveTuples;
            context.Response.Header = new()
            {
                Type = context.Request.Header.Type,
            };
            context.Response.Body = context.Request.Body;

            if (context.Request.Header.Type == DefaultPacketHeaderType.EchoByClient)
            {
                int remotePort;
                if (context.Request.PacketContext.FiveTuples.TransportType ==
                    ExclusiveTransportType.Udp)
                {
                    remotePort = context.Request.PacketContext.FiveTuples.RemotePort;
                }
                else
                {
                    remotePort = 8082;
                }
                context.Response.PacketContext.ClientConfig = new()
                {
                    ClientDisposeTimeout = TimeSpan.FromSeconds(2),
                    RemoteAddress = context.Request.PacketContext.FiveTuples.RemoteIp.ToString(),
                    RemotePort = remotePort,
                    TransportType = context.Request.PacketContext.FiveTuples.TransportType,
                };
            }

            this.defaultPipeline.GoUp(context);
        }
    }
}
