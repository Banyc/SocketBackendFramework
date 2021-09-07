using SocketBackendFramework.Relay.Models.Workflows;
using SocketBackendFramework.Relay.Pipeline;

namespace SocketBackendFramework.Relay.Workflow
{
    public abstract class WorkflowBuilder
    {
        protected readonly WorkflowConfig config;
        private readonly List<PipelineDomain> pipelineDomains = new();

        protected WorkflowBuilder(WorkflowConfig config)
        {
            this.config = config;
        }

        public Workflow Build()
        {
            ConfigurateWorkflow(this.config);
            return new(this.config, this.pipelineDomains);
        }

        protected void AddPipelineDomain(PipelineDomain pipelineDomain)
        {
            this.pipelineDomains.Add(pipelineDomain);
        }

        protected abstract void ConfigurateWorkflow(WorkflowConfig config);
    }
}
