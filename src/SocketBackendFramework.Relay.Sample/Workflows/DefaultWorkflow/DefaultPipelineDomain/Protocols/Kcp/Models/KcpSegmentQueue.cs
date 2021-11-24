using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models
{
    public class KcpSegmentQueue
    {
        // enqueue to last
        // dequeue from first
        private readonly LinkedList<KcpSegment> queue = new();

        public uint NextContiguousSequenceNumber { get; private set; } = 0;
        public uint SmallestSequenceNumberAllowed
        {
            get
            {
                uint? firstSequenceNumber = this.GetFirstSequenceNumber();
                if (firstSequenceNumber == null)
                {
                    return this.NextContiguousSequenceNumber;
                }
                else
                {
                    return firstSequenceNumber.Value;
                }
            }
        }  // the left inclusive boundary of the queue
        public int Count { get => this.queue.Count; }
        public int TotalByteCount { get; private set; } = 0;

        public KcpSegmentQueue()
        {
        }

        public void RemoveAllBefore(uint sequenceNumber)
        {
            var item = this.queue.First;
            while (item != null)
            {
                var next = item.Next;
                if (item.Value.SequenceNumber < sequenceNumber)
                {
                    this.queue.Remove(item);
                    this.TotalByteCount -= (int)item.Value.DataLength + (int)KcpSegment.DataOffset;
                }
                else
                {
                    break;
                }
                item = next;
            }
        }

        public void Remove(uint sequenceNumber)
        {
            var item = this.queue.First;
            while (item != null)
            {
                var next = item.Next;
                if (item.Value.SequenceNumber == sequenceNumber)
                {
                    this.queue.Remove(item);
                    this.TotalByteCount -= (int)item.Value.DataLength + (int)KcpSegment.DataOffset;
                    break;
                }
                if (item.Value.SequenceNumber > sequenceNumber)
                {
                    // the acknowledged segment is not in the send queue
                    break;
                }
                item = next;
            }
        }

        public void Remove(LinkedListNode<KcpSegment> node)
        {
            this.queue.Remove(node);
        }

        public void AddBuffer(Span<byte> buffer, uint maxSegmentDataSize, bool isStreamMode)
        {
            // the goal is to stuff the data in the send queue
            // the data should be segmented into KCP segments

            // append the first segment in input data to the last segment in the send queue
            if (isStreamMode &&
                this.queue.Count > 0 &&
                this.queue.Last!.Value.Command == Command.Push &&
                this.queue.Last!.Value.DataLength + buffer.Length <= maxSegmentDataSize)
            {
                int numBytesAppended = this.queue.Last.Value.Append(buffer);
                this.TotalByteCount += numBytesAppended;
                if (numBytesAppended == buffer.Length)
                {
                    return;
                }
                buffer = buffer[numBytesAppended..];
            }

            // then segment the remaining buffer into KCP segments
            int fragmentCountLeft = (int)Math.Ceiling((double)buffer.Length / maxSegmentDataSize);
            while (buffer.Length > 0)
            {
                int numBytesToAppend = Math.Min(buffer.Length, (int)maxSegmentDataSize);
                int segmentDataSize = isStreamMode ? (int)maxSegmentDataSize : numBytesToAppend;
                System.Diagnostics.Debug.Assert(segmentDataSize > 0);
                KcpSegment segment = new KcpSegment((uint)segmentDataSize)
                {
                    Command = Command.Push,
                    FragmentCountLeft = isStreamMode ? (byte)0 : (byte)Math.Min(fragmentCountLeft, byte.MaxValue),
                };
                int numBytesAppended = segment.Append(buffer[..numBytesToAppend]);
                this.Enqueue(segment);
                this.TotalByteCount += numBytesAppended + (int)KcpSegment.DataOffset;
                buffer = buffer[numBytesAppended..];
                fragmentCountLeft--;
            }
        }

        public void Enqueue(KcpSegment segment)
        {
            this.queue.AddLast(segment);
            this.TotalByteCount += (int)segment.DataLength + (int)KcpSegment.DataOffset;

            // update the next contiguous sequence number
            this.NextContiguousSequenceNumber = segment.SequenceNumber + 1;
        }
        public KcpSegment? Dequeue()
        {
            if (this.queue.Count == 0)
            {
                return null;
            }
            System.Diagnostics.Debug.Assert(this.queue.First != null);
            var segment = this.queue.First.Value;
            this.queue.RemoveFirst();
            this.TotalByteCount -= (int)segment.DataLength + (int)KcpSegment.DataOffset;

            return segment;
        }

        public uint? GetFirstSequenceNumber()
        {
            if (this.queue.Count == 0)
            {
                return null;
            }
            System.Diagnostics.Debug.Assert(this.queue.First != null);
            return this.queue.First.Value.SequenceNumber;
        }
        public uint? GetLastSequenceNumber()
        {
            if (this.queue.Count == 0)
            {
                return null;
            }
            System.Diagnostics.Debug.Assert(this.queue.Last != null);
            return this.queue.Last.Value.SequenceNumber;
        }
        public LinkedListNode<KcpSegment>? GetFirstNode()
        {
            if (this.queue.Count == 0)
            {
                return null;
            }
            return this.queue.First;
        }
        public LinkedListNode<KcpSegment>? GetLastNode()
        {
            if (this.queue.Count == 0)
            {
                return null;
            }
            return this.queue.Last;
        }
    }
}
