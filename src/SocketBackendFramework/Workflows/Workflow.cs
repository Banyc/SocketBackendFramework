using SocketBackendFramework.Listeners;
using SocketBackendFramework.Models.Workflows;

namespace SocketBackendFramework.Workflows
{
    public abstract class Workflow
    {
        public string Name { get => this.config.Name; }
        protected readonly WorkflowConfig config;

        public Workflow(WorkflowConfig config)
        {
            this.config = config;
        }

        public abstract void Start();
    }

    public class Workflow<TMiddlewareContext> : Workflow
    {
        private readonly ListenersMapper<TMiddlewareContext> listenersMapper;

        public Workflow(WorkflowConfig config,
                        ListenersMapper<TMiddlewareContext> listenersMapper)
            : base(config)
        {
            this.listenersMapper = listenersMapper;
        }

        public override void Start()
        {
            this.listenersMapper.Start();
        }
    }
}
