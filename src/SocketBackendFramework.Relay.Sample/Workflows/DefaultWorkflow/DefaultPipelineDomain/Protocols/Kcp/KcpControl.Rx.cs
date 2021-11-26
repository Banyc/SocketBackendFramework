using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp
{
    public partial class KcpControl  // Rx
    {
        private bool isNoDelayAck;

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

        public void Input(Span<byte> rawData)
        {
            lock (this)
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
                                // this specific segment is acknowledged and should be removed
                                this.sendingQueue.Remove(segment.SequenceNumber);
                            }
                            break;
                        case Command.Push:
                            {
                                // application data

                                // discard invalid segments
                                if ((int)segment.SequenceNumber - (int)this.receivedQueue.SmallestSequenceNumberAllowed > this.receiveWindowSize)
                                {
                                    // this segment is out of the receive window
                                    // discard it
                                    continue;
                                }

                                // pending to ack
                                this.pendingAckList.AddLast(new SequenceNumberTimestampPair()
                                {
                                    SequenceNumber = segment.SequenceNumber,
                                    Timestamp = segment.Timestamp,
                                });

                                // discard duplicate segments
                                if (segment.SequenceNumber < this.NextContiguousSequenceNumberToReceive)
                                {
                                    // this segment is a duplicate
                                    // discard it
                                    continue;
                                }

                                if (segment.SequenceNumber == this.NextContiguousSequenceNumberToReceive)
                                {
                                    // this segment is the next contiguous segment to receive
                                    // add it to the received queue
                                    this.receivedQueue.Enqueue(segment);

                                    if (segment.FragmentCountLeft == 0)
                                    {
                                        // this is the last segment of the message
                                        this.ReceivedNewSegment?.Invoke(this, EventArgs.Empty);
                                    }
                                }
                                else
                                {
                                    System.Diagnostics.Debug.Assert(segment.SequenceNumber > this.NextContiguousSequenceNumberToReceive);
                                    System.Diagnostics.Debug.Assert(segment.SequenceNumber < this.NextContiguousSequenceNumberToReceive + this.receiveWindowSize);
                                    // this segment is not the next contiguous segment to receive
                                    // this segment is out of order
                                    // add it to the out-of-order queue
                                    // implicitly discard duplicate segments
                                    this.outOfOrderQueue[segment.SequenceNumber] = segment;
                                }

                                // if the next consecutive segments is in the out-of-order queue, add them to the received queue
                                while (this.outOfOrderQueue.TryGetValue(this.NextContiguousSequenceNumberToReceive, out KcpSegment? nextSegment))
                                {
                                    System.Diagnostics.Debug.Assert(nextSegment != null);
                                    this.outOfOrderQueue.Remove(this.NextContiguousSequenceNumberToReceive);
                                    this.receivedQueue.Enqueue(nextSegment);

                                    if (nextSegment.FragmentCountLeft == 0)
                                    {
                                        // this is the last segment of the message
                                        this.ReceivedNewSegment?.Invoke(this, EventArgs.Empty);
                                    }
                                }

                                if (this.isNoDelayAck)
                                {
                                    // send ack immediately
                                    this.TryOutputAll();
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
                            // break;
                    }
                }
            }
        }

        public int Receive(Span<byte> buffer)
        {
            lock (this)
            {
                int numBytesAppended = 0;
                // merge all segments into a single buffer
                while (true)
                {
                    LinkedListNode<KcpSegment>? segmentNode = this.receivedQueue.GetFirstNode();
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
        }
    }
}
