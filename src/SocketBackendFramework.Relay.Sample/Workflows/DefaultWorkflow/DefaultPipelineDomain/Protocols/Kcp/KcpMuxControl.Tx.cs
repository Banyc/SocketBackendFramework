using System;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp
{
    public partial class KcpMuxControl  // Tx
    {
        private readonly object txLock = new();

        public uint Mtu { get; set; } = 1400;  // maximum transmission unit
        private readonly Random random = new();

        // public void Send(uint conversationId, Span<byte> data)
        // {
        //     lock (this.txLock)
        //     {
        //         this.kcpControls[conversationId].Send(data);
        //     }
        // }

        public int Output(Span<byte> buffer)
        {
            lock (this.txLock)
            {
                // only stuff bytes within the limit of MTU
                buffer = buffer[..Math.Min(buffer.Length, (int)this.Mtu)];

                int numBytesAppended = 0;
                
                lock (this.kcpControls)
                {
                    int maxRound = this.kcpControls.Count;
                    for (int i = 0; i < maxRound && numBytesAppended < buffer.Length; i++)
                    {
                        int offset = this.random.Next(this.kcpControls.Count);
                        numBytesAppended += this.kcpControls[this.baseConversationId + (uint)offset].Output(buffer[numBytesAppended..]);
                    }
                }

                return numBytesAppended;
            }
        }
    }
}
