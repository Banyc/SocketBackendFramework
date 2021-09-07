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

        public abstract PipelineDomain<TMiddlewareContext> Build();

        protected PipelineDomain<TMiddlewareContext> Build(Pipeline<TMiddlewareContext> pipeline,
                                                           IContextAdaptor<TMiddlewareContext> contextAdaptor)
        {
            ConfigurateMiddlewares(pipeline);

            TransportMapper<TMiddlewareContext> TransportMapper =
                new(config.TransportMapper, pipeline, contextAdaptor);

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
