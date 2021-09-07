using SocketBackendFramework.Relay.Models.Workflows;
using SocketBackendFramework.Relay.Pipeline;

namespace SocketBackendFramework.Relay.Workflow
{
    public class Workflow
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
    }
}
