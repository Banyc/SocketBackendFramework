using SocketBackendFramework.Middlewares;
using SocketBackendFramework.Middlewares.Codec;
using SocketBackendFramework.Models.Workflows;
using SocketBackendFramework.Workflows;
using SocketBackendFramework.Sample.Codec;
using SocketBackendFramework.Middlewares.ControllersMapper;
using SocketBackendFramework.Middlewares.ContextAdaptor;

namespace SocketBackendFramework.Sample.Workflows
{
    public class DefaultWorkflowBuilder : WorkflowBuilder
    {
        public DefaultWorkflowBuilder(WorkflowConfig config,
                                      PipelineBuilder pipelineBuilder,
                                      IContextAdaptor contextAdaptor)
            : base(config, pipelineBuilder, contextAdaptor)
        {
        }

        protected override void ConfigurateMiddlewares(PipelineBuilder pipelineBuilder)
        {
            // register the headerCodec
            IHeaderCodec headerCodec = new DefaultHeaderCodec();
            Middlewares.Codec.Codec codec = new(headerCodec);
            pipelineBuilder.UseMiddleware(codec);

            // register the controllers
            ControllersMapper controllersMapper = new();
            pipelineBuilder.UseMiddleware(controllersMapper);
        }
    }
}
