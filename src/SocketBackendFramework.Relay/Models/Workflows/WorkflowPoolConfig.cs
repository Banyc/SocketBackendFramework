using System.Collections.Generic;

namespace SocketBackendFramework.Relay.Models.Workflows
{
    public class WorkflowPoolConfig
    {
        public List<WorkflowConfig> Workflows { get; set; } = new();
    }
}
