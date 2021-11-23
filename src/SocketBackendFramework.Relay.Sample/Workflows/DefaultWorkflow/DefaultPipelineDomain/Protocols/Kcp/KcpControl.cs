using System;
using System.Collections.Generic;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp
{
    public enum ProbeCommand
    {
        WindowProbe = 1 << 0,
        WindowSize = 1 << 1,
    }

    public class KcpControl
    {
        private struct SequenceNumberTimestampPair
        {
            public uint SequenceNumber;
            public uint Timestamp;
        }

        private readonly uint conversationId;
        public uint Mtu { get; set; } = 1400;  // maximum transmission unit
        private uint MaxSegmentDataSize { get => this.Mtu - KcpSegment.DataOffset; }  // maximum segment size
        private TimeSpan RetransmissionTimeout { get; } = TimeSpan.FromSeconds(3);  // rto

        private uint nextSequenceNumberToSend = 0;  // snd_nxt

        // window size (out-of-order queue size)
        private readonly uint receiveWindowSize;  // rcv_wnd  // out-of-order queue size
        private uint remoteWindowSize = 0;

        private uint CurrentTimestamp { get => (uint)(DateTime.Now.ToBinary() >> 32); }  // current

        // unsent segments
        private readonly KcpSegmentQueue toSendQueue = new();  // snd_queue

        // sent but unacked segments
        private readonly KcpSegmentQueue sendingQueue = new();  // snd_buf

        // acked but out-of-order segments
        private readonly SortedDictionary<uint, KcpSegment> outOfOrderQueue = new();  // rcv_buf

        // acked and consecutive segments
        private readonly KcpSegmentQueue receivedQueue = new();  // rcv_queue
        private uint NextContiguousSequenceNumberToReceive
        {
            get
            {
                return this.receivedQueue.NextContiguousSequenceNumber;
            }
        }  // rcv_nxt

        private int SpaceLeftInOutOfOrderQueue
        {
            get
            {
                return Math.Min((int)this.receiveWindowSize - this.outOfOrderQueue.Count, 0);
            }
        }

        // sequence numbers to ack
        private readonly LinkedList<SequenceNumberTimestampPair> pendingAckList = new();  // ack list

        private ProbeCommand probeCommand;  // probe

        private bool isStreamMode;

        public KcpControl(uint conversationId,
                          bool isStreamMode,
                          uint receiveWindowSize)
        {
            this.conversationId = conversationId;
            this.isStreamMode = isStreamMode;
            this.receiveWindowSize = receiveWindowSize;
        }

        public void Input(Span<byte> rawData)
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

                if (segment.ConversationId != this.conversationId)
                {
                    throw new Exception("segment conversation id is not equal to the KCP conversation id");
                }
                if (segment.Data.Length < segment.DataLength)
                {
                    throw new Exception("raw packet length is not large enough to hold all data of the KCP segment");
                }
                if (segment.Command < Command.Push || segment.Command > Command.Ack)
                {
                    throw new Exception("segment command is not valid");
                }
                rawData = rawData[(int)(KcpSegment.DataOffset + segment.DataLength)..];

                // the packet is valid, process it

                this.remoteWindowSize = segment.WindowSize;


                // all segments that are previous to the unacknowledged number should be removed
                this.sendingQueue.RemoveAllBefore(segment.UnacknowledgedNumber);

                switch (segment.Command)
                {
                    case Command.Ack:
                        {
                            if (this.CurrentTimestamp >= segment.Timestamp)
                            {
                            }
                            // this specific segment is acknowledged and should be removed
                            this.sendingQueue.Remove(segment.SequenceNumber);
                        }
                        break;
                    case Command.Push:
                        {
                            // application data

                            // discard invalid segments
                            if ((int)segment.SequenceNumber - (int)this.NextContiguousSequenceNumberToReceive > this.receiveWindowSize)
                            {
                                // this segment is out of the receive window
                                // discard it
                                continue;
                            }

                            if (segment.SequenceNumber == this.NextContiguousSequenceNumberToReceive)
                            {
                                // this segment is the next contiguous segment to receive
                                // add it to the received queue
                                this.receivedQueue.Enqueue(segment);
                            }
                            else
                            {
                                // this segment is not the next contiguous segment to receive
                                // add it to the out-of-order queue
                                // implicitly discard duplicate segments
                                this.outOfOrderQueue[segment.SequenceNumber] = segment;
                            }

                            // pending to ack
                            this.pendingAckList.AddLast(new SequenceNumberTimestampPair()
                            {
                                SequenceNumber = segment.SequenceNumber,
                                Timestamp = segment.Timestamp,
                            });

                            // if the next consecutive segments is in the out-of-order queue, add them to the received queue
                            while (this.outOfOrderQueue.TryGetValue(this.NextContiguousSequenceNumberToReceive, out KcpSegment nextSegment))
                            {
                                this.outOfOrderQueue.Remove(this.NextContiguousSequenceNumberToReceive);
                                this.receivedQueue.Enqueue(nextSegment);
                            }
                        }
                        break;
                    case Command.WindowProbe:
                        {

                        }
                        break;
                    case Command.WindowSize:
                        {

                        }
                        break;
                    default:
                        {
                            throw new Exception("segment command is not valid");
                        }
                        break;
                }
            }
        }

        public void Send(Span<byte> data)
        {
            this.toSendQueue.AddBuffer(data, this.MaxSegmentDataSize, this.isStreamMode);
        }

        public int Receive(Span<byte> buffer)
        {
            int numBytesAppended = 0;
            // merge all segments into a single buffer
            while (true)
            {
                LinkedListNode<KcpSegment> segmentNode = this.receivedQueue.GetFirstNode();
                if (segmentNode == null)
                {
                    // no more segments in received queue
                    break;
                }
                KcpSegment segment = segmentNode.Value;

                if (numBytesAppended + segment.Data.Length > buffer.Length)
                {
                    // the buffer is not large enough to hold all data
                    break;
                }

                if (segment.FragmentCountLeft > this.receivedQueue.Count)
                {
                    // the segment is not complete
                    break;
                }

                segment.Data.CopyTo(buffer[numBytesAppended..]);
                numBytesAppended += segment.Data.Length;

                this.receivedQueue.Remove(segmentNode);

                if (segment.FragmentCountLeft == 0)
                {
                    // this is the last segment/fragment of the packet
                    break;
                }
            }
            return numBytesAppended;
        }

        public int Output(Span<byte> buffer)
        {
            // only stuff bytes within the limit of MTU
            buffer = buffer[..Math.Min(buffer.Length, (int)this.Mtu)];

            int numBytesAppended = 0;

            // write ack segments
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
                uint? firstToSendSequenceNumber = this.toSendQueue.GetFirstSequenceNumber();
                if (firstToSendSequenceNumber != null &&
                    firstToSendSequenceNumber.Value - this.sendingQueue.SmallestSequenceNumberAllowed < Math.Max(this.remoteWindowSize, 1))
                {
                    var segment = this.toSendQueue.Dequeue();

                    // initialize segment headers
                    segment.ConversationId = this.conversationId;
                    segment.Command = Command.Push;
                    // segment.FragmentCount = segment.FragmentCount;
                    segment.WindowSize = (ushort)this.SpaceLeftInOutOfOrderQueue;
                    segment.UnacknowledgedNumber = this.NextContiguousSequenceNumberToReceive;
                    // segment.DataLength = segment.DataLength;
                    segment.SequenceNumber = this.nextSequenceNumberToSend++;
                    segment.Timestamp = this.CurrentTimestamp;

                    this.sendingQueue.Enqueue(segment);
                }
                else
                {
                    break;
                }
            }

            // write push segments
            // merge all segments in sendingQueue into a single buffer
            {
                LinkedListNode<KcpSegment>? segmentNode = this.sendingQueue.GetFirstNode();
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
