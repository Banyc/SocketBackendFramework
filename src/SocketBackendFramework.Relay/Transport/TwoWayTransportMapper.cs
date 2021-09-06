using SocketBackendFramework.Relay.ContextAdaptor;
using SocketBackendFramework.Relay.Models;
using SocketBackendFramework.Relay.Models.Transport;
using SocketBackendFramework.Relay.Transport.Listeners;
using SocketBackendFramework.Relay.TwoWayPipeline;

namespace SocketBackendFramework.Relay.Transport
{
    public class TwoWayTransportMapper<TMiddlewareContext>
    {
        // port -> listener
        private readonly Dictionary<int, Listener> listeners = new();
        private readonly TwoWayPipeline<TMiddlewareContext> pipeline;
        private readonly IContextAdaptor<TMiddlewareContext> contextAdaptor;

        public TwoWayTransportMapper(TransportMapperConfig config, TwoWayPipeline<TMiddlewareContext> pipeline, IContextAdaptor<TMiddlewareContext> contextAdaptor)
        {
            foreach (var listenerConfig in config.ListenerConfigs)
            {
                Listener newListener = new(listenerConfig);
                newListener.PacketReceived += OnReceivePacket;
                this.listeners[listenerConfig.ListeningPort] = newListener;
            }

            this.pipeline = pipeline;
            pipeline.GoneUp += this.OnSendingPacket;
            this.contextAdaptor = contextAdaptor;
        }

        public void Start()
        {
            foreach ((_, Listener listener) in this.listeners)
            {
                listener.Start();
            }
        }

        private void OnReceivePacket(object sender, PacketContext context)
        {
            TMiddlewareContext middlewareContext = this.contextAdaptor.GetMiddlewareContext(context);
            this.pipeline.GoDown(middlewareContext);
        }

        private void OnSendingPacket(object sender, TMiddlewareContext middlewareContext)
        {
            PacketContext context = this.contextAdaptor.GetPacketContext(middlewareContext);
            this.listeners[context.LocalPort].Respond(context);
        }
    }
}
