using SocketBackendFramework.Middlewares;
using SocketBackendFramework.Middlewares.Codec;
using SocketBackendFramework.Models.Workflows;
using SocketBackendFramework.Workflows;
using SocketBackendFramework.Sample.Codec;
using SocketBackendFramework.Middlewares.ControllersMapper;
using SocketBackendFramework.Sample.Models;
using SocketBackendFramework.Sample.Helpers;

namespace SocketBackendFramework.Sample.Workflows
{
    public class DefaultWorkflowBuilder : WorkflowBuilder<MiddlewareContext>
    {
        public DefaultWorkflowBuilder(WorkflowConfig config)
            : base(config)
        {
        }

        public override Workflow Build()
        {
            PipelineBuilder<MiddlewareContext> pipelineBuilder = new();

            return base.Build(pipelineBuilder, new ContextAdaptor());
        }

        protected override void ConfigurateMiddlewares(PipelineBuilder<MiddlewareContext> pipelineBuilder)
        {
            // register the headerCodec
            IHeaderCodec<MiddlewareContext> headerCodec = new DefaultHeaderCodec();
            Middlewares.Codec.Codec<MiddlewareContext> codec = new(headerCodec);
            pipelineBuilder.UseMiddleware(codec);

            // register the controllers
            ControllersMapper<MiddlewareContext> controllersMapper = new();
            pipelineBuilder.UseMiddleware(controllersMapper);
        }
    }
}
