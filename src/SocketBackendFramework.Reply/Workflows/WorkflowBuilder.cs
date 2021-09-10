using SocketBackendFramework.Reply.Listeners;
using SocketBackendFramework.Reply.Middlewares;
using SocketBackendFramework.Reply.Middlewares.ContextAdaptor;
using SocketBackendFramework.Reply.Models.Workflows;

namespace SocketBackendFramework.Reply.Workflows
{
    public interface IWorkflowBuilder
    {
        Workflow Build();
    }

    public abstract class WorkflowBuilder<TMiddlewareContext> : IWorkflowBuilder
    {
        private readonly WorkflowConfig config;
        protected WorkflowBuilder(WorkflowConfig config)
        {
            this.config = config;
        }

        public abstract Workflow Build();

        protected Workflow Build(PipelineBuilder<TMiddlewareContext> pipelineBuilder,
                                 IContextAdaptor<TMiddlewareContext> contextAdaptor)
        {
            ConfigurateMiddlewares(pipelineBuilder);

            Pipeline<TMiddlewareContext> pipeline = new()
            {
                Entry = pipelineBuilder.Build()
            };
            ListenersMapper<TMiddlewareContext> listenersMapper =
                new(config.ListenersMapperConfig, pipeline, contextAdaptor);

            Workflow<TMiddlewareContext> workflow = new(
                this.config,
                listenersMapper
            );
            return workflow;
        }

        protected abstract void ConfigurateMiddlewares(PipelineBuilder<TMiddlewareContext> pipelineBuilder);
    }
}
