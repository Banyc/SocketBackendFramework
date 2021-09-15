using System;
using SocketBackendFramework.Relay.Models.Pipeline;
using SocketBackendFramework.Relay.Transport;

namespace SocketBackendFramework.Relay.Pipeline
{
    public abstract class PipelineDomain : IDisposable
    {
        protected readonly PipelineDomainConfig config;

        protected PipelineDomain(PipelineDomainConfig config)
        {
            this.config = config;
        }

        public abstract void Dispose();

        public abstract void Start();
    }

    public class PipelineDomain<TMiddlewareContext> : PipelineDomain
    {
        public Pipeline<TMiddlewareContext> Pipeline { get; }
        private readonly TransportMapper<TMiddlewareContext> transportMapper;

        public PipelineDomain(PipelineDomainConfig config,
                              TransportMapper<TMiddlewareContext> transportMapper,
                              Pipeline<TMiddlewareContext> pipeline)
            : base(config)
        {
            this.transportMapper = transportMapper;
            this.Pipeline = pipeline;
        }

        public override void Start()
        {
            this.transportMapper.Start();
        }

        public override void Dispose()
        {
            this.transportMapper.Dispose();
        }
    }
}
