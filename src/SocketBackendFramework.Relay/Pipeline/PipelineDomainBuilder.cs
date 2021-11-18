using System.Collections.Generic;
using SocketBackendFramework.Relay.ContextAdaptor;
using SocketBackendFramework.Relay.Models.Pipeline;
using SocketBackendFramework.Relay.Transport;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;
using SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers;

namespace SocketBackendFramework.Relay.Pipeline
{
    public abstract class PipelineDomainBuilder<TMiddlewareContext>
    {
        private readonly PipelineDomainConfig config;

        protected PipelineDomainBuilder(PipelineDomainConfig config)
        {
            this.config = config;
        }

        public abstract PipelineDomain<TMiddlewareContext> Build();

        protected PipelineDomain<TMiddlewareContext> Build(Pipeline<TMiddlewareContext> pipeline,
                                                           IContextAdaptor<TMiddlewareContext> contextAdaptor,
                                                           Dictionary<string, IServerHandlerBuilder> serverHandlerBuilders,
                                                           Dictionary<string, IClientHandlerBuilder> clientHandlerBuilders)
        {
            ConfigurateMiddlewares(pipeline);

            TransportMapper<TMiddlewareContext> TransportMapper =
                new(config.TransportMapper, serverHandlerBuilders, clientHandlerBuilders, pipeline, contextAdaptor);

            PipelineDomain<TMiddlewareContext> pipelineDomain = new(
                this.config,
                TransportMapper,
                pipeline
            );
            return pipelineDomain;
        }

        protected abstract void ConfigurateMiddlewares(Pipeline<TMiddlewareContext> pipeline);
    }
}
