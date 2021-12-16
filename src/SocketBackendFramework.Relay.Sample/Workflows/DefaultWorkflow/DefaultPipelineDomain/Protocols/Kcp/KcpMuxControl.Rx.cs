using System;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp
{
    public partial class KcpMuxControl  // Rx
    {
        private readonly object rxLock = new();

        public void Input(Span<byte> rawData)
        {
            lock (this.rxLock)
            {
                while (true)
                {
                    if (rawData.Length == 0)
                    {
                        // no more data
                        break;
                    }
                    if (rawData.Length < KcpSegment.DataOffset)
                    {
                        throw new Exception("raw packet length is less than data offset");
                    }

                    KcpSegment segment = new KcpSegment(rawData);

                    if (segment.DataWriteBuffer.Length < segment.DataLength)
                    {
                        throw new Exception("raw packet length is not large enough to hold all data of the KCP segment");
                    }
                    if (segment.Command < Command.Push || segment.Command > Command.Ack)
                    {
                        throw new Exception("segment command is not valid");
                    }
                    rawData = rawData[(int)(KcpSegment.DataOffset + segment.DataLength)..];

                    // the packet is valid, process it
                    System.Diagnostics.Debug.WriteLine($"[KcpMuxControl {this.Name}] KcpControl {segment.ConversationId} received {segment.Command} with {segment.DataLength} bytes");
                    lock (this.kcpControls)
                    {
                        if (this.kcpControls.Count != 0)
                        {
                            this.kcpControls[segment.ConversationId].Input(segment);
                        }
                        // else it means this is disposed.
                    }
                }
            }
        }
        // public int Receie(uint conversationId, Span<byte> buffer)
        // {
        //     lock (this.rxLock)
        //     {
        //         return this.kcpControls[conversationId].Receive(buffer);
        //     }
        // }
    }
}
