using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Timers;
using SocketBackendFramework.Relay.Models.Delegates;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp;
using SocketBackendFramework.Relay.Transport.Listeners.SocketHandlers;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.DefaultSocketHandlers
{
    public class KcpServerHandlerBuilderConfig
    {
        public Dictionary<string, KcpServerHandlerConfig>? KcpServerHandlers { get; set; }
    }
    public class KcpServerHandlerBuilder : IServerHandlerBuilder
    {
        private readonly KcpServerHandlerBuilderConfig config;
        public KcpServerHandlerBuilder(KcpServerHandlerBuilderConfig config)
        {
            this.config = config;
        }
        public IServerHandler Build(IPEndPoint localEndPoint, string? configId)
        {
            KcpControlBuilder kcpControlBuilder = new KcpControlBuilder();
            return new KcpServerHandler(kcpControlBuilder, localEndPoint, this.config.KcpServerHandlers![configId!]);
        }
    }

    public class KcpServerHandlerConfig
    {
        public double ConnectionTimeoutMs { get; set; }
    }
    public class KcpServerHandler: NetCoreServer.UdpServer, IServerHandler
    {
        private struct ClientInfo : IDisposable
        {
            public KcpControl KcpControl { get; set; }
            public Timer TimeoutTimer { get; set; }

            public void Dispose()
            {
                TimeoutTimer.Stop();
                TimeoutTimer.Dispose();
                KcpControl.Dispose();
            }
        }

        public string TransportType => "kcp";

        public EndPoint LocalEndPoint { get; }

        public event ConnectionEventHandler? ClientConnected { add { } remove { } }
        public event ConnectionEventHandler? ClientDisconnected;
        public event ReceivedEventHandler? ClientMessageReceived;

        private readonly byte[] receiveBuffer = new byte[1400 * 256];
        private readonly byte[] sendBuffer = new byte[1400 * 256];

        private readonly KcpControlBuilder kcpControlBuilder;
        private readonly ConcurrentDictionary<EndPoint, ClientInfo> connections = new();
        private readonly TimeSpan connectionTimeout;

        public KcpServerHandler(KcpControlBuilder kcpControlBuilder, IPEndPoint localEndPoint, KcpServerHandlerConfig config) : base(localEndPoint)
        {
            this.kcpControlBuilder = kcpControlBuilder;
            this.LocalEndPoint = localEndPoint;
            this.connectionTimeout = TimeSpan.FromMilliseconds(config.ConnectionTimeoutMs);
        }

        protected override void OnStarted()
        {
            base.OnStarted();
            base.ReceiveAsync();
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            KcpControl selectedKcpControl;
            if (this.connections.TryGetValue(endpoint, out var clientInfo))
            {
                selectedKcpControl = clientInfo.KcpControl;
                clientInfo.TimeoutTimer.Stop();
                clientInfo.TimeoutTimer.Start();
            }
            else
            {
                var kcpControl = this.kcpControlBuilder.Build();
                kcpControl.ReceivedCompleteSegment += (sender, completeSegmentBatchCount) =>
                {
                    int i;
                    for (i = 0; i < completeSegmentBatchCount; i++)
                    {
                        int readByteCount = kcpControl.Receive(this.receiveBuffer);
                        this.ClientMessageReceived?.Invoke(
                            this,
                            this.TransportType,
                            this.LocalEndPoint!,
                            endpoint!,
                            this.receiveBuffer,
                            0,
                            readByteCount
                        );
                    }
                };
                kcpControl.TryingOutput += (sender, e) =>
                {
                    lock (this.sendBuffer)
                    {
                        int writtenByteCount = kcpControl.Output(this.sendBuffer);
                        while (writtenByteCount > 0)
                        {
                            base.SendAsync(endpoint, this.sendBuffer, 0, writtenByteCount);
                            writtenByteCount = kcpControl.Output(this.sendBuffer);
                        }
                    }
                };

                // TODO: add timeout timer
                Timer timeoutTimer = new Timer(this.connectionTimeout.TotalMilliseconds);
                timeoutTimer.Elapsed += (sender, e) =>
                {
                    this.connections.TryRemove(endpoint, out _);
                    this.ClientDisconnected?.Invoke(
                        this,
                        this.TransportType,
                        this.LocalEndPoint!,
                        endpoint!
                    );
                };

                ClientInfo newClientInfo = new()
                {
                    KcpControl = kcpControl,
                    TimeoutTimer = timeoutTimer
                };
                this.connections.TryAdd(endpoint, newClientInfo);

                selectedKcpControl = kcpControl;
            }
            selectedKcpControl.Input(buffer.AsSpan()[(int)offset..(int)(offset + size)]);

            base.ReceiveAsync();
        }

        #region IServerHandler
        void IServerHandler.Start()
        {
            base.Start();
        }

        void IServerHandler.Send(EndPoint remoteEndPoint, byte[] buffer, long offset, long size)
        {
            // base.SendAsync(remoteEndPoint, buffer, offset, size);
            if (this.connections.TryGetValue(remoteEndPoint, out var clientInfo))
            {
                clientInfo.KcpControl.Send(buffer.AsSpan()[(int)offset..(int)(offset + size)]);
                clientInfo.TimeoutTimer.Stop();
                clientInfo.TimeoutTimer.Start();
            }
            else
            {
                throw new ArgumentException("Client not connected");
            }
        }

        public void Disconnect(EndPoint remoteEndPoint)
        {    
            this.connections.TryRemove(remoteEndPoint, out ClientInfo clientInfo);
            clientInfo.Dispose();
        }

        void IDisposable.Dispose()
        {
            base.Stop();
            while (!this.connections.IsEmpty)
            {
                (EndPoint remoteEndPoint, _) = this.connections.First();
                this.Disconnect(remoteEndPoint);
            }
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
