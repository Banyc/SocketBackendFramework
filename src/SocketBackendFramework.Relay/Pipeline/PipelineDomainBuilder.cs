using SocketBackendFramework.Relay.ContextAdaptor;
using SocketBackendFramework.Relay.Models.Pipeline;
using SocketBackendFramework.Relay.Transport;

namespace SocketBackendFramework.Relay.Pipeline
{
    public abstract class PipelineDomainBuilder<TMiddlewareContext>
    {
        private readonly PipelineDomainConfig config;

        protected PipelineDomainBuilder(PipelineDomainConfig config)
        {
            this.config = config;
        }

        protected abstract PipelineDomain<TMiddlewareContext> Build();

        protected PipelineDomain<TMiddlewareContext> Build(PipelineBuilder<TMiddlewareContext> pipelineBuilder,
                                                           IContextAdaptor<TMiddlewareContext> contextAdaptor)
        {
            ConfigurateMiddlewares(pipelineBuilder);

            Pipeline<TMiddlewareContext> pipeline = pipelineBuilder.Build();
            TransportMapper<TMiddlewareContext> TransportMapper =
                new(config.TransportMapper, pipeline, contextAdaptor);

            PipelineDomain<TMiddlewareContext> pipelineDomain = new(
                this.config,
                TransportMapper
            );
            return pipelineDomain;
        }

        protected abstract void ConfigurateMiddlewares(PipelineBuilder<TMiddlewareContext> pipelineBuilder);
    }
}
