using System.Collections.Generic;
using SocketBackendFramework.Middlewares;
using SocketBackendFramework.Middlewares.ContextAdaptor;
using SocketBackendFramework.Models;
using SocketBackendFramework.Models.Listeners;
using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Listeners
{
    public class ListenersMapper<TMiddlewareContext>
    {
        // port -> listener
        private readonly Dictionary<int, Listener> listeners = new();
        private readonly Pipeline<TMiddlewareContext> pipeline;
        private readonly IContextAdaptor<TMiddlewareContext> contextAdaptor;

        public ListenersMapper(ListenersMapperConfig config, Pipeline<TMiddlewareContext> pipeline, IContextAdaptor<TMiddlewareContext> contextAdaptor)
        {
            foreach (var listenerConfig in config.ListenerConfigs)
            {
                Listener newListener = new(listenerConfig);
                newListener.PacketReceived += OnReceivePacket;
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

        private void OnReceivePacket(object sender, PacketContext context)
        {
            TMiddlewareContext middlewareContext = this.contextAdaptor.GetMiddlewareContext(context);
            this.pipeline.Entry(middlewareContext);

            context = this.contextAdaptor.GetPacketContext(middlewareContext);
            this.listeners[context.LocalPort].Respond(context);
        }
    }
}
