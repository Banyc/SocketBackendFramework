using System.Collections.Generic;
using SocketBackendFramework.Middlewares;
using SocketBackendFramework.Middlewares.ContextAdaptor;
using SocketBackendFramework.Models;
using SocketBackendFramework.Models.Listeners;
using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Listeners
{
    public class ListenersMapper
    {
        // port -> listener
        private readonly Dictionary<int, Listener> listeners = new();
        private readonly Pipeline pipeline;
        private readonly IContextAdaptor contextAdaptor;

        public ListenersMapper(ListenersMapperConfig config, Pipeline pipeline, IContextAdaptor contextAdaptor)
        {
            foreach (var listenerConfig in config.ListenerConfigs)
            {
                Listener newListener = new(listenerConfig, this);
                this.listeners[listenerConfig.ListeningPort] = newListener;
            }

            this.pipeline = pipeline;
            this.contextAdaptor = contextAdaptor;
        }

        public void Start()
        {
            foreach ((_, Listener listener) in this.listeners)
            {
                listener.Start();
            }
        }

        public void OnReceivePacket(PacketContext context)
        {
            IMiddlewareContext middlewareContext = this.contextAdaptor.GetMiddlewareContext(context);
            this.pipeline.Entry(middlewareContext);

            context = this.contextAdaptor.GetPacketContext(middlewareContext);
            this.listeners[context.LocalPort].Respond(context);
        }
    }
}
