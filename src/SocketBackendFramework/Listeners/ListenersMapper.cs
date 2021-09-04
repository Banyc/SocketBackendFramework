using System.Collections.Generic;
using SocketBackendFramework.Middlewares;
using SocketBackendFramework.Models;
using SocketBackendFramework.Models.Listeners;

namespace SocketBackendFramework.Listeners
{
    public class ListenersMapper
    {
        // port -> listener
        private readonly Dictionary<int, Listener> listeners = new();
        private readonly Pipeline pipeline;

        public ListenersMapper(ListenersMapperConfig config, Pipeline pipeline)
        {
            foreach (var listenerConfig in config.ListenerConfigs)
            {
                Listener newListener = new(listenerConfig, this);
                this.listeners[listenerConfig.ListeningPort] = newListener;
            }

            this.pipeline = pipeline;
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
            this.pipeline.Entry(context);

            this.listeners[context.LocalPort].Respond(context);
        }
    }
}
