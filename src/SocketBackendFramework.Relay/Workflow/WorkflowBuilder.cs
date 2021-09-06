using SocketBackendFramework.Relay.Models.Workflows;

namespace SocketBackendFramework.Relay.Workflow
{
    public abstract class WorkflowBuilder
    {
        protected readonly WorkflowConfig config;
        protected WorkflowBuilder(WorkflowConfig config)
        {
            this.config = config;
        }

        protected abstract Workflow Build();
    }
}
