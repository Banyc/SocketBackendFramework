using System.Collections.Generic;
using SocketBackendFramework.Models.Workflows;

namespace SocketBackendFramework.Workflows
{
    // Build workflows
    public class WorkflowPool
    {
        private readonly WorkflowPoolConfig config;
        private readonly List<Workflow> workflows = new();

        public WorkflowPool(WorkflowPoolConfig config)
        {
            this.config = config;
        }

        public WorkflowConfig GetWorkflowConfig(string workflowName)
        {
            return this.config.WorkflowConfigs.Find(xxxx => xxxx.Name == workflowName);
        }

        public void AddWorkflow(WorkflowBuilder workflowBuilder)
        {
            this.workflows.Add(workflowBuilder.Build());
        }

        public void Start()
        {
            foreach (var workflow in this.workflows)
            {
                workflow.Start();
            }
        }
    }
}
