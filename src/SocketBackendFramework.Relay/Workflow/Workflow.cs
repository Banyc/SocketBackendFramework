using SocketBackendFramework.Relay.Models.Workflows;
using SocketBackendFramework.Relay.Transport;

namespace SocketBackendFramework.Relay.Workflow
{
    public class Workflow
    {
        public string Name { get => this.config.Name; }

        private readonly WorkflowConfig config;
        private readonly List<TransportMapper> transportMappers;

        public Workflow(WorkflowConfig config, List<TransportMapper> transportMappers)
        {
            this.config = config;
            this.transportMappers = transportMappers;
        }

        public void Start()
        {
            foreach (var transportMapper in this.transportMappers)
            {
                transportMapper.Start();
            }
        }
    }
}
