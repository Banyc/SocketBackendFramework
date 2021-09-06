using SocketBackendFramework.Reply.Middlewares;
using SocketBackendFramework.Reply.Middlewares.Codec;
using SocketBackendFramework.Reply.Models.Workflows;
using SocketBackendFramework.Reply.Workflows;
using SocketBackendFramework.Reply.Sample.Codec;
using SocketBackendFramework.Reply.Middlewares.ControllersMapper;
using SocketBackendFramework.Reply.Sample.Models;
using SocketBackendFramework.Reply.Sample.Helpers;
using SocketBackendFramework.Reply.Sample.Controllers;

namespace SocketBackendFramework.Reply.Sample.Workflows
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
            controllersMapper.AddController(new NoReplyController());
            controllersMapper.AddController(new EchoController());
            pipelineBuilder.UseMiddleware(controllersMapper);
        }
    }
}
