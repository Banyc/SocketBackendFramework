using SocketBackendFramework.Listeners;
using SocketBackendFramework.Models.Workflows;

namespace SocketBackendFramework.Workflows
{
    public class Workflow
    {
        public string Name { get => this.config.Name; }
        private readonly WorkflowConfig config;
        private readonly ListenersMapper listenersMapper;

        public Workflow(WorkflowConfig config,
                        ListenersMapper listenersMapper)
        {
            this.config = config;
            this.listenersMapper = listenersMapper;
        }

        public void Start()
        {
            this.listenersMapper.Start();
        }
    }
}
