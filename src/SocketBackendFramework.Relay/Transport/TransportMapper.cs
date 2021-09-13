using System.Collections.Concurrent;
using SocketBackendFramework.Relay.ContextAdaptor;
using SocketBackendFramework.Relay.Models;
using SocketBackendFramework.Relay.Models.Transport;
using SocketBackendFramework.Relay.Pipeline;
using SocketBackendFramework.Relay.Transport.Clients;
using SocketBackendFramework.Relay.Transport.Listeners;

namespace SocketBackendFramework.Relay.Transport
{
    public abstract class TransportMapper : IDisposable
    {
        // local port -> listener
        protected readonly Dictionary<int, Listener> listeners = new();
        // local port -> client
        protected readonly ConcurrentDictionary<int, TransportClient> clients = new();

        protected TransportMapper(TransportMapperConfig config)
        {
            foreach (var listenerConfig in config.Listeners)
            {
                Listener newListener = new(listenerConfig, this.transportAgentIdCounter++);
                newListener.PacketReceived += OnReceivePacket;
                newListener.TcpSessionDisconnected += this.OnReceivePacket;
                this.listeners[listenerConfig.ListeningPort] = newListener;
            }
        }

        private uint transportAgentIdCounter = 0;

        public void Start()
        {
            foreach ((_, Listener listener) in this.listeners)
            {
                listener.Start();
            }
        }

        // pass packet context down to pipeline
        protected abstract void OnReceivePacket(object sender, PacketContext context);

        // receive packet context from pipeline
        protected void OnSendingPacket(object sender, PacketContext context)
        {
            switch (context.PacketContextType)
            {
                case PacketContextType.ApplicationMessaging:
                    SendApplicationMessage(sender, context);
                    break;
                case PacketContextType.Disconnecting:
                    ActivelyDisconnect(sender, context);
                    break;
                default:
                    throw new ArgumentException();
                    break;
            }
        }

        private void ActivelyDisconnect(object sender, PacketContext context)
        {
            if (this.listeners.ContainsKey(context.LocalPort))
            {
                this.listeners[context.LocalPort].DisconnectTcpSession(context);
            }
            else if (this.clients.ContainsKey(context.LocalPort))
            {
                // don't dispose client since it will trigger a disposal process on the disconnection event.
                this.clients[context.LocalPort].Disconnect();
            }
            // else, the tcp session or the client might be disposed and removed from the list.
        }

        private void SendApplicationMessage(object sender, PacketContext context)
        {
            if (context.ClientConfig == null)
            {
                if (this.listeners.ContainsKey(context.LocalPort))
                {
                    this.listeners[context.LocalPort].Respond(context);
                }
                else
                {
                    this.clients[context.LocalPort].Respond(context);
                }
            }
            else
            {
                // create a dedicated client to send the packet
                TransportClient newClient = new(context.ClientConfig, this.transportAgentIdCounter++);
                newClient.PacketReceived += OnReceivePacket;
                void DisposeClient(object sender)
                {
                    TransportClient client = (TransportClient)sender;
                    int localPort = client.LocalIPEndPoint.Port;
                    this.clients.Remove(localPort, out _);
                    client.Dispose();
                }
                newClient.Disconnected += (sender, context) =>
                {
                    // dispose client before sending the event to pipeline
                    DisposeClient(sender);
                    this.OnReceivePacket(sender, context);
                };
                newClient.ClientTimedOut += sender => {
                    TransportClient client = (TransportClient)sender;
                    client.Disconnect();
                };
                newClient.Respond(context);
                this.clients[newClient.LocalIPEndPoint.Port] = newClient;
            }
        }

        public void Dispose()
        {
            foreach (var (_, listener) in this.listeners)
            {
                listener.Dispose();
            }
            foreach (var (_, client) in this.clients)
            {
                client.Dispose();
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
