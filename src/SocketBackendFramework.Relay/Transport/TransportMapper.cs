using System.Collections.Concurrent;
using SocketBackendFramework.Relay.ContextAdaptor;
using SocketBackendFramework.Relay.Models;
using SocketBackendFramework.Relay.Models.Transport;
using SocketBackendFramework.Relay.Pipeline;
using SocketBackendFramework.Relay.Transport.Clients;
using SocketBackendFramework.Relay.Transport.Listeners;

namespace SocketBackendFramework.Relay.Transport
{
    public abstract class TransportMapper
    {
        // local port -> listener
        protected readonly Dictionary<int, Listener> listeners = new();
        // local port -> client
        protected readonly ConcurrentDictionary<int, TransportClient> clients = new();

        protected TransportMapper(TransportMapperConfig config)
        {
            foreach (var listenerConfig in config.Listeners)
            {
                Listener newListener = new(listenerConfig);
                newListener.PacketReceived += OnReceivePacket;
                this.listeners[listenerConfig.ListeningPort] = newListener;
            }
        }

        public void Start()
        {
            foreach ((_, Listener listener) in this.listeners)
            {
                listener.Start();
            }
        }

        protected abstract void OnReceivePacket(object sender, PacketContext context);

        protected void OnSendingPacket(object sender, PacketContext context)
        {
            if (context.LocalPort != null && context.ClientConfig == null)
            {
                if (this.listeners.ContainsKey(context.LocalPort.Value))
                {
                    this.listeners[context.LocalPort.Value].Respond(context);
                }
                else
                {
                    this.clients[context.LocalPort.Value].Respond(context);
                }
            }
            else
            {
                // create a dedicated client to send the packet
                TransportClient newClient = new(context.ClientConfig);
                newClient.PacketReceived += OnReceivePacket;
                void DisposeClient(object sender)
                {
                    TransportClient client = (TransportClient)sender;
                    int localPort = client.LocalPort;
                    this.clients.Remove(localPort, out _);
                    client.Dispose();
                }
                newClient.TcpClientDisconnected += DisposeClient;
                newClient.ClientTimedOut += DisposeClient;
                newClient.Respond(context);
                this.clients[newClient.LocalPort] = newClient;
            }
        }
    }

    public class TransportMapper<TMiddlewareContext> : TransportMapper
    {
        private readonly Pipeline<TMiddlewareContext> pipeline;
        private readonly IContextAdaptor<TMiddlewareContext> contextAdaptor;

        public TransportMapper(TransportMapperConfig config, Pipeline<TMiddlewareContext> pipeline, IContextAdaptor<TMiddlewareContext> contextAdaptor)
            : base(config)
        {
            this.pipeline = pipeline;
            pipeline.GoneUp += this.OnSendingPacket;
            this.contextAdaptor = contextAdaptor;
        }

        protected override void OnReceivePacket(object sender, PacketContext context)
        {
            TMiddlewareContext middlewareContext = this.contextAdaptor.GetMiddlewareContext(context);
            this.pipeline.GoDown(middlewareContext);
        }

        private void OnSendingPacket(object sender, TMiddlewareContext middlewareContext)
        {
            PacketContext context = this.contextAdaptor.GetPacketContext(middlewareContext);
            base.OnSendingPacket(sender, context);
        }
    }
}
