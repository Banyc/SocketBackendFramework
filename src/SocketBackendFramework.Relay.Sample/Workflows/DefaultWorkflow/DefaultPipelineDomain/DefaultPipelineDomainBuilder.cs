using System.Collections.Generic;
using SocketBackendFramework.Relay.Models.Pipeline;
using SocketBackendFramework.Relay.Pipeline;
using SocketBackendFramework.Relay.Pipeline.Middlewares.Codec;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Middlewares;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;
using SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain
{
    public class DefaultPipelineDomainBuilder : PipelineDomainBuilder<DefaultMiddlewareContext>
    {
        public DefaultPipelineDomainBuilder(PipelineDomainConfig config) : base(config)
        {
        }

        public override PipelineDomain<DefaultMiddlewareContext> Build()
        {
            // build specialized pipeline
            Pipeline<DefaultMiddlewareContext> pipeline = new();
            // setup transport layer
            Dictionary<string, IServerHandlerBuilder> serverHandlerBuilders = new()
            {
                { "tcp", new TcpServerHandlerBuilder() },
                { "udp", new UdpServerHandlerBuilder() },
            };
            Dictionary<string, IClientHandlerBuilder> clientHandlerBuilders = new()
            {
                { "tcp", new TcpClientHandlerBuilder() },
                { "udp", new UdpClientHandlerBuilder() },
            };

            // inject components to pipeline domain
            return base.Build(pipeline, new DefaultContextAdaptor(), serverHandlerBuilders, clientHandlerBuilders);
        }

        protected override void ConfigurateMiddlewares(Pipeline<DefaultMiddlewareContext> pipeline)
        {
            // register the headerCodec
            IHeaderCodec<DefaultMiddlewareContext> headerCodec = new DefaultHeaderCodec();
            Codec<DefaultMiddlewareContext> codec = new(headerCodec);
            pipeline.Use(codec);
        }
    }
}
