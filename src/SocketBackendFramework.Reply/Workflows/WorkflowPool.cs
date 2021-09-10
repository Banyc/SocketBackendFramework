using System.Collections.Generic;
using SocketBackendFramework.Reply.Models.Workflows;

namespace SocketBackendFramework.Reply.Workflows
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

        public void AddWorkflow(Workflow workflow)
        {
            this.workflows.Add(workflow);
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
