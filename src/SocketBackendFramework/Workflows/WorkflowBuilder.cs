using SocketBackendFramework.Listeners;
using SocketBackendFramework.Middlewares;
using SocketBackendFramework.Models.Workflows;

namespace SocketBackendFramework.Workflows
{
    public abstract class WorkflowBuilder
    {
        private readonly WorkflowConfig config;
        private readonly PipelineBuilder pipelineBuilder;

        protected WorkflowBuilder(WorkflowConfig config,
                                  PipelineBuilder pipelineBuilder)
        {
            this.config = config;
            this.pipelineBuilder = pipelineBuilder;
        }

        public Workflow Build()
        {
            ConfigurateMiddlewares(pipelineBuilder);

            Pipeline pipeline = new()
            {
                Entry = pipelineBuilder.Build()
            };
            ListenersMapper listenersMapper = new(config.ListenersMapperConfig, pipeline);

            Workflow workflow = new(
                config,
                listenersMapper
            );
            return workflow;
        }

        protected abstract void ConfigurateMiddlewares(PipelineBuilder pipelineBuilder);
    }
}
