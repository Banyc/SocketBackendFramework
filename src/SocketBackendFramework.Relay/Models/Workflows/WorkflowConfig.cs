using System.Linq;
using System.Collections.Generic;
using SocketBackendFramework.Relay.Models.Pipeline;

namespace SocketBackendFramework.Relay.Models.Workflows
{
    public class WorkflowConfig
    {
        public List<PipelineDomainConfig> PipelineDomains { get; set; } = new();
        public string Name { get; set; } = "undefined";

        public PipelineDomainConfig GetPipelineDomainConfig(string name)
        {
            return this.PipelineDomains.Single(xxxx => xxxx.Name == name);
        }
    }
}
