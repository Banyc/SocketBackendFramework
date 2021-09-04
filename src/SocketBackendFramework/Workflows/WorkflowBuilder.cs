using SocketBackendFramework.Listeners;
using SocketBackendFramework.Middlewares;
using SocketBackendFramework.Middlewares.ContextAdaptor;
using SocketBackendFramework.Models.Workflows;

namespace SocketBackendFramework.Workflows
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
