using SocketBackendFramework.Listeners;
using SocketBackendFramework.Middlewares;
using SocketBackendFramework.Middlewares.ContextAdaptor;
using SocketBackendFramework.Models.Workflows;

namespace SocketBackendFramework.Workflows
{
    public abstract class WorkflowBuilder
    {
        private readonly WorkflowConfig config;
        private readonly PipelineBuilder pipelineBuilder;
        private readonly IContextAdaptor contextAdaptor;

        protected WorkflowBuilder(WorkflowConfig config,
                                  PipelineBuilder pipelineBuilder,
                                  IContextAdaptor contextAdaptor)
        {
            this.config = config;
            this.pipelineBuilder = pipelineBuilder;
            this.contextAdaptor = contextAdaptor;
        }

        public Workflow Build()
        {
            ConfigurateMiddlewares(pipelineBuilder);

            Pipeline pipeline = new()
            {
                Entry = pipelineBuilder.Build()
            };
            ListenersMapper listenersMapper =
                new(config.ListenersMapperConfig, pipeline, contextAdaptor);

            Workflow workflow = new(
                config,
                listenersMapper
            );
            return workflow;
        }

        protected abstract void ConfigurateMiddlewares(PipelineBuilder pipelineBuilder);
    }
}
