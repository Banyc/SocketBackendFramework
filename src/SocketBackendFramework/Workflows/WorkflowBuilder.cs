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
        private readonly PipelineBuilder<TMiddlewareContext> pipelineBuilder;
        private readonly IContextAdaptor<TMiddlewareContext> contextAdaptor;

        protected WorkflowBuilder(WorkflowConfig config,
                                  PipelineBuilder<TMiddlewareContext> pipelineBuilder,
                                  IContextAdaptor<TMiddlewareContext> contextAdaptor)
        {
            this.config = config;
            this.pipelineBuilder = pipelineBuilder;
            this.contextAdaptor = contextAdaptor;
        }

        public Workflow Build()
        {
            ConfigurateMiddlewares(pipelineBuilder);

            Pipeline<TMiddlewareContext> pipeline = new()
            {
                Entry = pipelineBuilder.Build()
            };
            ListenersMapper<TMiddlewareContext> listenersMapper =
                new(config.ListenersMapperConfig, pipeline, contextAdaptor);

            Workflow<TMiddlewareContext> workflow = new(
                config,
                listenersMapper
            );
            return workflow;
        }

        protected abstract void ConfigurateMiddlewares(PipelineBuilder<TMiddlewareContext> pipelineBuilder);
    }
}
