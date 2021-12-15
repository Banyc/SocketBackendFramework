using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp
{
    public partial class KcpControl  // Tx
    {
        private readonly object txLock = new();

        public bool IsStreamMode { get; }
        public uint Mtu { get; set; } = 1400;  // maximum transmission unit
        private uint MaxSegmentDataSize { get => this.Mtu - KcpSegment.DataOffset; }  // maximum segment size
        private TimeSpan RetransmissionTimeout { get; } = TimeSpan.FromSeconds(3);  // rto
        private uint nextSequenceNumberToSend = 0;  // snd_nxt
        private int SpaceLeftInOutOfOrderQueue
        {
            get
            {
                lock (this.outOfOrderQueue)
                {
                    return Math.Max((int)this.receiveWindowSize - this.outOfOrderQueue.Count, 0);
                }
            }
        }

        private bool IsFitInSendingQueue(uint sequenceNumber)
        {
            lock (this.sendingQueue)
            {
                // even if the remote window size is 0, we still send one segment to probe the updated remote window size
                return sequenceNumber - this.sendingQueue.SmallestSequenceNumberAllowed < Math.Max(this.remoteWindowSize, 1);
            }
        }

        // unsent segments
        private readonly KcpSegmentQueue toSendQueue = new();  // snd_queue

        public void Send(Span<byte> data)
        {
            bool shouldTryOutputAll = false;
            lock (this.txLock)
            {
                this.toSendQueue.AddBuffer(data, this.MaxSegmentDataSize, this.IsStreamMode);

                // check if MTU is reached and good to transmit bytes
                uint firstSequenceNumber = this.nextSequenceNumberToSend;
                uint lastSequenceNumber = this.nextSequenceNumberToSend + (uint)this.toSendQueue.Count - 1;
                if (this.IsFitInSendingQueue(firstSequenceNumber) &&  // sending queue is not full
                    (
                        this.shouldSendSmallPacketsNoDelay ||
                        (
                            this.toSendQueue.TotalByteCount >= this.Mtu ||  // MTU is reached
                            !this.IsFitInSendingQueue(lastSequenceNumber + 1)))  // to fully fill the sending queue
                    )
                {
                    shouldTryOutputAll = true;
                }
            }
            if (shouldTryOutputAll)
            {
                // detach KCP output from user thread
                _ = this.TryOutputAsync(shouldStartNewTask: true);
            }
        }

        public int Output(Span<byte> buffer)
        {
            lock (this.txLock)
            {
                // only stuff bytes within the limit of MTU
                buffer = buffer[..Math.Min(buffer.Length, (int)this.Mtu)];

                int numBytesAppended = 0;

                // write ack segments
                // piggyback ack segments
                lock (this.pendingAckList)
                {
                    var node = this.pendingAckList.First;
                    while (node != null)
                    {
                        if (numBytesAppended + KcpSegment.DataOffset > buffer.Length)
                        {
                            // not enough space in TX buffer to write the next segment
                            return numBytesAppended;
                        }

                        // construct ack segment
                        KcpSegment segment = new KcpSegment(0)
                        {
                            ConversationId = this.conversationId,
                            Command = Command.Ack,
                            FragmentCountLeft = 0,
                            WindowSize = (ushort)this.SpaceLeftInOutOfOrderQueue,
                            UnacknowledgedNumber = this.NextContiguousSequenceNumberToReceive,
                            DataLength = 0,
                            SequenceNumber = node.Value.SequenceNumber,
                            Timestamp = node.Value.Timestamp,
                        };

                        // write TX buffer
                        segment.Buffer.Span.CopyTo(buffer[numBytesAppended..]);
                        numBytesAppended += (int)KcpSegment.DataOffset;

                        // remove ack from pending ack list
                        var nextNode = node.Next;
                        this.pendingAckList.Remove(node);
                        node = nextNode;
                    }
                }

                // move just enough segments from toSend queue to sending queue
                while (true)
                {
                    if (this.IsFitInSendingQueue(this.nextSequenceNumberToSend) &&
                        this.toSendQueue.Count > 0)
                    {
                        var segment = this.toSendQueue.Dequeue();
                        System.Diagnostics.Debug.Assert(segment != null);

                        // initialize segment headers
                        segment.ConversationId = this.conversationId;
                        segment.Command = Command.Push;
                        // segment.FragmentCount = segment.FragmentCount;
                        segment.WindowSize = (ushort)this.SpaceLeftInOutOfOrderQueue;
                        segment.UnacknowledgedNumber = this.NextContiguousSequenceNumberToReceive;
                        // segment.DataLength = segment.DataLength;
                        segment.SequenceNumber = this.nextSequenceNumberToSend++;
                        segment.Timestamp = CurrentTimestamp;

                        lock (this.sendingQueue)
                        {
                            this.sendingQueue.Enqueue(segment);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                // write push segments
                // merge all segments in sendingQueue into a single buffer
                {
                    LinkedListNode<KcpSegment>? segmentNode;
                    lock (this.sendingQueue)
                    {
                        segmentNode = this.sendingQueue.GetFirstNode();
                    }
                    while (true)
                    {
                        if (segmentNode == null)
                        {
                            // no more segments to send
                            break;
                        }
                        KcpSegment segment = segmentNode.Value;

                        if (segment.LastSentTime != null &&
                            DateTime.Now - segment.LastSentTime.Value < this.RetransmissionTimeout)
                        {
                            // this segment has been sent recently
                            // do not `continue` here!
                        }
                        else
                        {
                            if (numBytesAppended + segment.Buffer.Length > buffer.Length)
                            {
                                // not enough space in TX buffer to write the next segment
                                break;
                            }

                            // write TX buffer
                            segment.RawSegment.CopyTo(buffer[numBytesAppended..]);
                            numBytesAppended += segment.RawSegmentLength;

                            // update segment last sent time
                            segment.LastSentTime = DateTime.Now;

                            // the segment is then waiting for ack
                        }

                        segmentNode = segmentNode.Next;
                    }
                }

                return numBytesAppended;
            }
        }
    }
}
