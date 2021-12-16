using System;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp
{
    public partial class KcpMuxControl  // Tx
    {
        private readonly object txLock = new();

        public uint Mtu { get; set; } = 1400;  // maximum transmission unit
        private readonly Random random = new();

        public int Output(Span<byte> buffer)
        {
            lock (this.txLock)
            {
                // only stuff bytes within the limit of MTU
                buffer = buffer[..Math.Min(buffer.Length, (int)this.Mtu)];

                int numBytesAppended = 0;

                lock (this.pendingOutputRequests)
                {
                    while (this.pendingOutputRequests.Count > 0 && numBytesAppended + KcpSegment.DataOffset < buffer.Length)
                    {
                        KcpControl kcpControl = this.pendingOutputRequests.Dequeue();

                        numBytesAppended += kcpControl.Output(buffer[numBytesAppended..]);
                    }
                }

                return numBytesAppended;
            }
        }
    }
}
