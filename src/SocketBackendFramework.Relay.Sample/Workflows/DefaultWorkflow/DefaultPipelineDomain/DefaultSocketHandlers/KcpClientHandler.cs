using System;
using System.Net;
using SocketBackendFramework.Relay.Models.Delegates;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models;
using SocketBackendFramework.Relay.Transport.Clients.SocketHandlers;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.DefaultSocketHandlers
{
    public class KcpClientHandlerBuilder : IClientHandlerBuilder
    {
        public IClientHandler Build(string ipAddress, int port, object? config)
        {
            KcpConfig kcpConfig = (KcpConfig)config!;
            KcpControl kcpControl = new(kcpConfig);
            return new KcpClientHandler(kcpControl, ipAddress, port);
        }
    }

    public class KcpClientHandler : NetCoreServer.UdpClient, IClientHandler
    {
        private readonly KcpControl kcpControl;

        public event ConnectionEventHandler? Connected;
        public event ReceivedEventHandler? Received;
        public event ConnectionEventHandler? Disconnected;

        public string TransportType => "kcp";
        public EndPoint? LocalEndPoint { get; private set; }
        public EndPoint? RemoteEndPoint { get; private set; }

        private readonly byte[] receiveBuffer = new byte[1400 * 256];
        private readonly byte[] sendBuffer = new byte[1400 * 256];

        public KcpClientHandler(KcpControl kcpControl, string ipAddr, int port) : base(ipAddr, port)
        {
            this.kcpControl = kcpControl;
            kcpControl.ReceivedCompleteSegment += (sender, completeSegmentBatchCount) =>
            {
                int i;
                for (i = 0; i < completeSegmentBatchCount; i++)
                {
                    int readByteCount = this.kcpControl.Receive(this.receiveBuffer);
                    this.Received?.Invoke(
                        this,
                        this.TransportType,
                        this.LocalEndPoint!,
                        this.RemoteEndPoint!,
                        this.receiveBuffer,
                        0,
                        readByteCount
                    );
                }
            };
            kcpControl.TryingOutput += (sender, e) =>
            {
                int writtenByteCount = this.kcpControl.Output(this.sendBuffer);
                while (writtenByteCount > 0)
                {
                    base.SendAsync(this.sendBuffer, 0, writtenByteCount);
                    writtenByteCount = this.kcpControl.Output(this.sendBuffer);
                }
            };
            this.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddr), port);
        }

        void IClientHandler.Connect()
        {
            base.Connect();
        }

        void IClientHandler.Disconnect()
        {
            base.Disconnect();
        }

        void IClientHandler.Send(byte[] buffer, long offset, long size)
        {
            // base.SendAsync(buffer, offset, size);
            this.kcpControl.Send(buffer.AsSpan()[(int)offset..(int)(offset + size)]);
        }

        void IDisposable.Dispose()
        {
            this.kcpControl.Dispose();
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        protected override void OnConnected()
        {
            // cache the endpoint info
            this.LocalEndPoint = base.Socket!.LocalEndPoint!;
            this.RemoteEndPoint = base.Socket!.RemoteEndPoint!;

            this.Connected?.Invoke(
                this,
                this.TransportType,
                this.LocalEndPoint,
                this.RemoteEndPoint);
            base.ReceiveAsync();  // correspond to official sample
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            this.kcpControl.Input(buffer.AsSpan()[(int)offset..(int)(offset + size)]);
            base.ReceiveAsync();
        }

        protected override void OnDisconnected()
        {
            this.Disconnected?.Invoke(
                this,
                this.TransportType,
                this.LocalEndPoint!,
                this.RemoteEndPoint!);
        }
    }
}
