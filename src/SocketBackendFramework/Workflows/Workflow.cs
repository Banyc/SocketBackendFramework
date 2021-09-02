using SocketBackendFramework.Listeners;
using SocketBackendFramework.Middlewares;
using SocketBackendFramework.Models.Workflows;

namespace SocketBackendFramework.Workflows
{
    public class Workflow
    {
        private readonly WorkflowConfig config;
        private readonly ListenersMapper listenersMapper;

        public Workflow(WorkflowConfig config,
                        PipelineBuilder pipelineBuilder)
        {
            this.config = config;
            this.listenersMapper = new(config.ListenersMapperConfig, pipelineBuilder);
        }

        public void Start()
        {
            this.listenersMapper.Start();
        }
    }
}
