using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocketBackendFramework.Relay.Models.Workflows;
using SocketBackendFramework.Relay.Pipeline;

namespace SocketBackendFramework.Relay.Workflows
{
    public class Workflow : IDisposable
    {
        public string Name { get => this.config.Name; }

        private readonly WorkflowConfig config;
        private readonly List<PipelineDomain> PipelineDomains;

        public Workflow(WorkflowConfig config,
                        List<PipelineDomain> PipelineDomains)
        {
            this.config = config;
            this.PipelineDomains = PipelineDomains;
        }

        public void Start()
        {
            foreach (var PipelineDomain in this.PipelineDomains)
            {
                PipelineDomain.Start();
            }
        }

        public async Task RunAsync()
        {
            this.Start();
            await Task.Delay(-1);
        }

        public void Dispose()
        {
            foreach (var pipelineDomain in this.PipelineDomains)
            {
                pipelineDomain.Dispose();
            }
        }
    }
}
