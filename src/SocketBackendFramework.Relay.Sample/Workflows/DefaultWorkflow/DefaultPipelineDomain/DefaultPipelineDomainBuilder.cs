using SocketBackendFramework.Relay.Models.Pipeline;
using SocketBackendFramework.Relay.Pipeline;
using SocketBackendFramework.Relay.Pipeline.Middlewares.Codec;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Middlewares;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;

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
            // inject ContextAdaptor to pipeline
            return base.Build(pipeline, new DefaultContextAdaptor());
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
