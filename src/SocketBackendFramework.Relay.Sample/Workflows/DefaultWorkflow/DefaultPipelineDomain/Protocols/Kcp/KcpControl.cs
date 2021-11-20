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
        //
        private UInt32 conversationId;
        private UInt32 mtu = 1400;  // maximum transmission unit
        private UInt32 maxSegmentDataSize;  // maximum segment size

        // 
        private uint FirstSentUnacknowledged
        {
            get
            {
                uint? firstSentUnacknowledged = this.sentQueue.GetFirstSequenceNumber();
                if (firstSentUnacknowledged == null)
                {
                    return this.nextSequenceNumberToSend;
                }
                else
                {
                    return firstSentUnacknowledged.Value;
                }
            }
        }  // snd_una
        private UInt32 nextSequenceNumberToSend;  // snd_nxt
        private UInt32 receiveNext;  // rcv_nxt

        //
        private UInt32 sendWindowSize;  // snd_wnd
        private UInt32 receiveWindowSize;  // rcv_wnd


        private uint remoteWindowSize;  // rmt_wnd

        private uint currentTimestamp;  // current

        private readonly KcpSegmentQueue sendingQueue = new();  // snd_queue

        // unacked segments
        private readonly KcpSegmentQueue sentQueue = new();  // snd_buf

        // unacked segments
        private KcpSegmentQueue receivingQueue = new();  // rcv_buf

        private KcpSegmentQueue receivedQueue = new();  // rcv_queue


        private ProbeCommand probeCommand;  // probe

        private bool isFastResend;
        private bool isNoCwnd;
        private bool isStreamMode;

        public KcpControl(uint conversationId)
        {
            this.conversationId = conversationId;
        }

        public void Input(Span<byte> rawData)
        {
            bool hasProcessedFirstAck = false;

            while (true)
            {
                if (rawData.Length < KcpSegment.DataOffset)
                {
                    throw new Exception("raw packet length is less than data offset");
                }

                KcpSegment segment = new KcpSegment(rawData);

                if (segment.ConversationId != this.conversationId)
                {
                    throw new Exception("segment conversation id is not equal to the KCP conversation id");
                }
                if (segment.Data.Length < segment.Length)
                {
                    throw new Exception("raw packet length is not large enough to hold all data of the KCP segment");
                }
                if (segment.Command < Command.Push || segment.Command > Command.Ack)
                {
                    throw new Exception("segment command is not valid");
                }
                rawData = rawData[(int)(KcpSegment.DataOffset + segment.Length)..];

                // the packet is valid, process it

                this.remoteWindowSize = segment.WindowSize;

                
                // all segments that are previous to the unacknowledged number should be removed
                this.sentQueue.RemoveAllBefore(segment.UnacknowledgedNumber);

                switch (segment.Command)
                {
                    case Command.Ack:
                    {
                        if (this.currentTimestamp >= segment.Timestamp)
                        {
                        }
                        // this specific segment is acknowledged and should be removed
                        this.sentQueue.Remove(segment.SequenceNumber);

                        if (!hasProcessedFirstAck)
                        {
                            hasProcessedFirstAck = true;
                        }
                        else
                        {

                        }
                    }
                    break;
                    case Command.Push:
                    {
                        // application data

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
            this.sendingQueue.AddBuffer(data, this.maxSegmentDataSize, this.isStreamMode);
        }

        public int Receive(Span<byte> buffer)
        {
            // merge all segments into a single buffer
            int numBytesAppended = 0;
            while (true)
            {
                LinkedListNode<KcpSegment> segmentNode = this.receivedQueue.GetFirstNode();
                KcpSegment segment = segmentNode.Value;
                if (segment == null)
                {
                    break;
                }

                if (numBytesAppended + segment.Data.Length > buffer.Length)
                {
                    break;
                }

                segment.Data.CopyTo(buffer[numBytesAppended..]);
                numBytesAppended += segment.Data.Length;

                this.receivedQueue.Remove(segmentNode);
            }

            return numBytesAppended;
        }

        public int Output(Span<byte> buffer)
        {
            // merge all segments into a single buffer
            int numBytesAppended = 0;
            while (true)
            {
                LinkedListNode<KcpSegment> segmentNode = this.sendingQueue.GetFirstNode();
                KcpSegment segment = segmentNode.Value;
                if (segment == null)
                {
                    break;
                }

                if (numBytesAppended + segment.Data.Length > buffer.Length)
                {
                    break;
                }

                segment.Data.CopyTo(buffer[numBytesAppended..]);
                numBytesAppended += segment.Data.Length;

                this.sendingQueue.Remove(segmentNode);
                this.sentQueue.Enqueue(segment);
            }

            return numBytesAppended;
        }
    }
}
