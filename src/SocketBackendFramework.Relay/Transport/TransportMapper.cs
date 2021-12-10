using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SocketBackendFramework.Relay.ContextAdaptor;
using SocketBackendFramework.Relay.Models.Transport;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
using SocketBackendFramework.Relay.Pipeline;
using SocketBackendFramework.Relay.Transport.Clients;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;
using SocketBackendFramework.Relay.Transport.Listeners;
using SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers;

namespace SocketBackendFramework.Relay.Transport
{
    public abstract class TransportMapper : IDisposable
    {
        // local port -> listener
        protected readonly Dictionary<int, Listener> listeners = new();
        // local port -> client
        protected readonly ConcurrentDictionary<int, TransportClient> clients = new();

        // private readonly TransportMapperConfig config;
        private uint transportAgentIdCounter = 0;

        // private readonly Dictionary<string, IServerHandlerBuilder> serverBuilders;
        private readonly Dictionary<string, IClientHandlerBuilder> clientBuilders;

        protected TransportMapper(TransportMapperConfig config,
                                  Dictionary<string, IServerHandlerBuilder> serverBuilders,
                                  Dictionary<string, IClientHandlerBuilder> clientBuilders)
        {
            // this.config = config;
            // this.serverBuilders = serverBuilders;
            this.clientBuilders = clientBuilders;

            foreach (var listenerConfig in config.Listeners)
            {
                Listener newListener = new(listenerConfig, serverBuilders[listenerConfig.TransportType!], this.transportAgentIdCounter++);
                newListener.PacketReceived += this.OnDownwardEvent;
                newListener.TcpServerConnected += this.OnDownwardEvent;
                newListener.TcpSessionDisconnected += this.OnDownwardEvent;
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

        // pass packet context down to pipeline
        protected abstract void OnDownwardEvent(object? sender, DownwardPacketContext context);

        // receive packet context from pipeline
        protected void SendPacket(UpwardPacketContext context)
        {
            switch (context.ActionType)
            {
                case UpwardActionType.SendApplicationMessage:
                    SendApplicationMessage(context);
                    break;
                case UpwardActionType.Disconnect:
                    ActivelyDisconnectAsync(context);
                    break;
                default:
                    throw new ArgumentException();
                    // break;
            }
        }

        private void ActivelyDisconnectAsync(UpwardPacketContext context)
        {
            if (this.listeners.ContainsKey(context.FiveTuples!.Local!.Port))
            {
                this.listeners[context.FiveTuples.Local.Port].DisconnectTcpSession(context);
            }
            else if (this.clients.ContainsKey(context.FiveTuples.Local.Port))
            {
                // don't dispose client since it will trigger a disposal process on the disconnection event.
                this.clients[context.FiveTuples.Local.Port].DisconnectAsync();
            }
            // else, the tcp session or the client might be disposed and removed from the list.
        }

        private void SendApplicationMessage(UpwardPacketContext context)
        {
            if (context.ClientConfig == null)
            {
                if (this.listeners.ContainsKey(context.FiveTuples!.Local!.Port))
                {
                    this.listeners[context.FiveTuples.Local.Port].Respond(context);
                }
                else
                {
                    this.clients[context.FiveTuples.Local.Port].Respond(context);
                }
            }
            else
            {
                // create a dedicated client to send the packet
                TransportClient newClient = new(context.ClientConfig, this.clientBuilders[context.ClientConfig.TransportType!], this.transportAgentIdCounter++);
                newClient.Connected += (sender, transportType, localEndPoint, remoteEndPoint) => {
                    TransportClient transportClient = (TransportClient)sender;
                    this.clients[transportClient.LocalIPEndPoint!.Port] = transportClient;
                };
                newClient.PacketReceived += this.OnDownwardEvent;
                void DisposeClient(TransportClient client)
                {
                    int localPort = client.LocalIPEndPoint!.Port;
                    this.clients.Remove(localPort, out _);
                    client.Dispose();
                }
                newClient.Disconnected += (sender, context) =>
                {
                    // dispose client before sending the event to pipeline
                    DisposeClient(newClient);
                    this.OnDownwardEvent(sender, context);
                };
                newClient.Connect();
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
            GC.SuppressFinalize(this);
        }
    }

    public class TransportMapper<TMiddlewareContext> : TransportMapper
    {
        private readonly Pipeline<TMiddlewareContext> pipeline;
        private readonly IContextAdaptor<TMiddlewareContext> contextAdaptor;

        public TransportMapper(TransportMapperConfig config,
                               Dictionary<string, IServerHandlerBuilder> serverBuilders,
                               Dictionary<string, IClientHandlerBuilder> clientBuilders,
                               Pipeline<TMiddlewareContext> pipeline,
                               IContextAdaptor<TMiddlewareContext> contextAdaptor)
            : base(config, serverBuilders, clientBuilders)
        {
            this.pipeline = pipeline;
            pipeline.GoneUp += this.OnSendingPacket;
            this.contextAdaptor = contextAdaptor;
        }

        protected override void OnDownwardEvent(object? sender, DownwardPacketContext context)
        {
            TMiddlewareContext middlewareContext = this.contextAdaptor.GetMiddlewareContext(context);
            this.pipeline.GoDown(middlewareContext);
        }

        private void OnSendingPacket(object? sender, TMiddlewareContext middlewareContext)
        {
            UpwardPacketContext context = this.contextAdaptor.GetPacketContext(middlewareContext);
            base.SendPacket(context);
        }
    }
}
