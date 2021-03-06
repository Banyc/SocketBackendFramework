using System.Linq;
using System;
using System.Collections.Generic;
using SocketBackendFramework.Relay.Models.Workflows;

namespace SocketBackendFramework.Relay.Workflows
{
    // Build workflows
    public class WorkflowPool : IDisposable
    {
        private readonly WorkflowPoolConfig config;
        private readonly List<Workflow> workflows = new();

        public WorkflowPool(WorkflowPoolConfig config)
        {
            this.config = config;
        }

        public WorkflowConfig GetWorkflowConfig(string workflowName)
        {
            return this.config.Workflows.Single(xxxx => xxxx.Name == workflowName);
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

        public void Dispose()
        {
            foreach (var workflow in this.workflows)
            {
                workflow.Dispose();
            }
        }
    }
}
