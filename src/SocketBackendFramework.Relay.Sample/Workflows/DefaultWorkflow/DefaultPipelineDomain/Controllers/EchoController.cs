using System;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
using SocketBackendFramework.Relay.Pipeline;
using SocketBackendFramework.Relay.Pipeline.Middlewares.ControllersMapper.Controllers;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Controllers
{
    public class EchoController : Controller<DefaultMiddlewareContext>
    {
        private readonly Pipeline<DefaultMiddlewareContext> defaultPipeline;

        public EchoController(Pipeline<DefaultMiddlewareContext> defaultPipeline)
        {
            this.defaultPipeline = defaultPipeline;
        }

        public override bool IsControllerMatch(DefaultMiddlewareContext context)
        {
            return
                context.Request!.PacketContext!.EventType ==
                    DownwardEventType.ApplicationMessageReceived &&
                (
                    context.Request.Header.Type == DefaultPacketHeaderType.Echo ||
                    context.Request.Header.Type == DefaultPacketHeaderType.EchoByClient
                );
        }

        public override void Request(DefaultMiddlewareContext context)
        {
            context.Response = new();
            context.Response.PacketContext.ActionType = UpwardActionType.SendApplicationMessage;
            context.Response.PacketContext.FiveTuples = context.Request!.PacketContext!.FiveTuples;
            context.Response.Header = new()
            {
                Type = context.Request.Header.Type,
            };
            context.Response.Body = context.Request.Body;

            if (context.Request.Header.Type == DefaultPacketHeaderType.EchoByClient)
            {
                int remotePort;
                if (context.Request!.PacketContext!.FiveTuples!.TransportType == "udp")
                {
                    remotePort = context.Request!.PacketContext!.FiveTuples!.Remote!.Port;
                }
                else
                {
                    remotePort = 8082;
                }
                context.Response.PacketContext.NewClientConfig = new()
                {
                    ClientDisposeTimeout = TimeSpan.FromSeconds(2),
                    RemoteAddress = context.Request!.PacketContext!.FiveTuples!.Remote!.Address.ToString(),
                    RemotePort = remotePort,
                    TransportType = context.Request.PacketContext.FiveTuples.TransportType,
                };
            }

            this.defaultPipeline.GoUp(context);
        }
    }
}
