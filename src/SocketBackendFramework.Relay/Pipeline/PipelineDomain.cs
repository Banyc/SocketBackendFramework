using SocketBackendFramework.Relay.Models.Pipeline;
using SocketBackendFramework.Relay.Transport;

namespace SocketBackendFramework.Relay.Pipeline
{
    public abstract class PipelineDomain
    {
        protected readonly PipelineDomainConfig config;

        protected PipelineDomain(PipelineDomainConfig config)
        {
            this.config = config;
        }

        public abstract void Start();
    }

    public class PipelineDomain<TMiddlewareContext> : PipelineDomain
    {
        private readonly TransportMapper<TMiddlewareContext> transportMapper;

        public PipelineDomain(PipelineDomainConfig config,
                              TransportMapper<TMiddlewareContext> transportMapper)
            : base(config)
        {
            this.transportMapper = transportMapper;
        }

        public override void Start()
        {
            this.transportMapper.Start();
        }
    }
}
