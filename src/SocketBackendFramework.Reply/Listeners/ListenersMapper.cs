using System.Collections.Generic;
using SocketBackendFramework.Reply.Middlewares;
using SocketBackendFramework.Reply.Middlewares.ContextAdaptor;
using SocketBackendFramework.Reply.Models;
using SocketBackendFramework.Reply.Models.Listeners;

namespace SocketBackendFramework.Reply.Listeners
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
