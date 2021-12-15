using System;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
using SocketBackendFramework.Relay.Pipeline;
using SocketBackendFramework.Relay.Pipeline.Middlewares.ControllersMapper.Controllers;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Controllers
{
    public class RelayControllerConfig
    {
        public string? TransportType { get; set; }
        public string? RemoteAddress { get; set; }
        public int RemotePort { get; set; }
        public double ClientDisposeTimeoutMs { get; set; }
    }
    public class RelayController : Controller<DefaultMiddlewareContext>
    {
        private readonly RelayControllerConfig config;
        private readonly Pipeline<DefaultMiddlewareContext> defaultPipeline;
        public RelayController(RelayControllerConfig config, Pipeline<DefaultMiddlewareContext> defaultPipeline)
        {
            this.config = config;
            this.defaultPipeline = defaultPipeline;
        }

        public override bool IsControllerMatch(DefaultMiddlewareContext context)
        {
            if (context.Request!.Header.Type == DefaultPacketHeaderType.Relay)
            {
                return true;
            }
            return false;
        }

        public override void Request(DefaultMiddlewareContext context)
        {
            context.Response = new();
            context.Response.Header.Type = DefaultPacketHeaderType.Echo;
            context.Response.Body = context.Request!.Body;

            context.Response.PacketContext.ActionType = UpwardActionType.SendApplicationMessage;
            KcpConfig kcpConfig = new()
            {
                ConversationId = 0x0,
                IsStreamMode = false,
                ReceiveWindowSize = 15,
                ShouldSendSmallPacketsNoDelay = false,
                RetransmissionTimeout = TimeSpan.FromSeconds(3),
                OutputDuration = TimeSpan.FromMilliseconds(10),
            };
            context.Response.PacketContext.NewClientConfig = new()
            {
                TransportType = this.config.TransportType,
                RemoteAddress = this.config.RemoteAddress,
                RemotePort = this.config.RemotePort,
                ClientDisposeTimeout = TimeSpan.FromMilliseconds(this.config.ClientDisposeTimeoutMs),
                SocketHandlerConfig = kcpConfig,
            };

            this.defaultPipeline.GoUp(context);
        }
    }
}
