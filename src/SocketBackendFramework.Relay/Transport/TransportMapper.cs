using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SocketBackendFramework.Relay.ContextAdaptor;
using SocketBackendFramework.Relay.Models.Transport;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
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
                newListener.PacketReceived += OnDownwardEvent;
                newListener.TcpServerConnected += this.OnDownwardEvent;
                newListener.TcpSessionDisconnected += this.OnDownwardEvent;
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
        protected abstract void OnDownwardEvent(object sender, DownwardPacketContext context);

        // receive packet context from pipeline
        protected void OnSendingPacket(object sender, UpwardPacketContext context)
        {
            switch (context.ActionType)
            {
                case UpwardActionType.SendApplicationMessage:
                    SendApplicationMessage(sender, context);
                    break;
                case UpwardActionType.Disconnect:
                    ActivelyDisconnectAsync(sender, context);
                    break;
                default:
                    throw new ArgumentException();
                    break;
            }
        }

        private void ActivelyDisconnectAsync(object sender, UpwardPacketContext context)
        {
            if (this.listeners.ContainsKey(context.FiveTuples.LocalPort))
            {
                this.listeners[context.FiveTuples.LocalPort].DisconnectTcpSession(context);
            }
            else if (this.clients.ContainsKey(context.FiveTuples.LocalPort))
            {
                // don't dispose client since it will trigger a disposal process on the disconnection event.
                this.clients[context.FiveTuples.LocalPort].DisconnectAsync();
            }
            // else, the tcp session or the client might be disposed and removed from the list.
        }

        private void SendApplicationMessage(object sender, UpwardPacketContext context)
        {
            if (context.ClientConfig == null)
            {
                if (this.listeners.ContainsKey(context.FiveTuples.LocalPort))
                {
                    this.listeners[context.FiveTuples.LocalPort].Respond(context);
                }
                else
                {
                    this.clients[context.FiveTuples.LocalPort].Respond(context);
                }
            }
            else
            {
                // create a dedicated client to send the packet
                TransportClient newClient = new(context.ClientConfig, this.transportAgentIdCounter++);
                newClient.Connected += sender => {
                    TransportClient transportClient = (TransportClient)sender;
                    this.clients[transportClient.LocalIPEndPoint.Port] = transportClient;
                };
                newClient.PacketReceived += OnDownwardEvent;
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
                    this.OnDownwardEvent(sender, context);
                };
                newClient.ClientTimedOut += sender => {
                    TransportClient client = (TransportClient)sender;
                    client.DisconnectAsync();
                };
                newClient.Respond(context);
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

        protected override void OnDownwardEvent(object sender, DownwardPacketContext context)
        {
            TMiddlewareContext middlewareContext = this.contextAdaptor.GetMiddlewareContext(context);
            this.pipeline.GoDown(middlewareContext);
        }

        private void OnSendingPacket(object sender, TMiddlewareContext middlewareContext)
        {
            UpwardPacketContext context = this.contextAdaptor.GetPacketContext(middlewareContext);
            base.OnSendingPacket(sender, context);
        }
    }
}
