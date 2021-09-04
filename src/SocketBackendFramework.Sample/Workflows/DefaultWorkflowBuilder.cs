using SocketBackendFramework.Middlewares;
using SocketBackendFramework.Middlewares.Codec;
using SocketBackendFramework.Models.Workflows;
using SocketBackendFramework.Workflows;
using SocketBackendFramework.Sample.Codec;
using SocketBackendFramework.Middlewares.ControllersMapper;

namespace SocketBackendFramework.Sample.Workflows
{
    public class DefaultWorkflowBuilder : WorkflowBuilder
    {
        public DefaultWorkflowBuilder(WorkflowConfig config, PipelineBuilder pipelineBuilder) : base(config, pipelineBuilder)
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
