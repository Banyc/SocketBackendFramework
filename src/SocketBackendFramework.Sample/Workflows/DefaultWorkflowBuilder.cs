using SocketBackendFramework.Middlewares;
using SocketBackendFramework.Middlewares.Codec;
using SocketBackendFramework.Models.Workflows;
using SocketBackendFramework.Workflows;
using SocketBackendFramework.Sample.Codec;
using SocketBackendFramework.Middlewares.ControllersMapper;
using SocketBackendFramework.Middlewares.ContextAdaptor;
using SocketBackendFramework.Sample.Models;

namespace SocketBackendFramework.Sample.Workflows
{
    public class DefaultWorkflowBuilder : WorkflowBuilder<MiddlewareContext>
    {
        public DefaultWorkflowBuilder(WorkflowConfig config,
                                      PipelineBuilder<MiddlewareContext> pipelineBuilder,
                                      IContextAdaptor<MiddlewareContext> contextAdaptor)
            : base(config, pipelineBuilder, contextAdaptor)
        {
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
