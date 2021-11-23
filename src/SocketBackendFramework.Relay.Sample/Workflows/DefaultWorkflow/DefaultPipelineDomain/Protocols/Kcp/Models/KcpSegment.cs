using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models
{
    public enum Command
    {
        Push = 81,
        Ack = 82,
        WindowProbe = 83,  // ask
        WindowSize = 84,  // tell
    }

    // the packet structure

    // 0               4   5   6       8 (BYTE)
    // +---------------+---+---+-------+
    // |     conv      |cmd|frg|  wnd  |
    // +---------------+---+---+-------+   8
    // |     ts        |     sn        |
    // +---------------+---------------+  16
    // |     una       |     len       |
    // +---------------+---------------+  24
    // |                               |
    // |        DATA (optional)        |
    // |                               |
    // +-------------------------------+

    public class KcpSegment
    {
        public static uint DataOffset { get; } = 24;

        // header + data + extra space
        public Memory<byte> Buffer { get; }
        public uint ConversationId
        {
            get
            {
                return BitConverter.ToUInt32(Buffer.Span[..4]);
            }
            set
            {
                var segment = Buffer.Span[..4];
                segment[0] = (byte)(value & 0xff);
                segment[1] = (byte)((value >> 8) & 0xff);
                segment[2] = (byte)((value >> 16) & 0xff);
                segment[3] = (byte)((value >> 24) & 0xff);
            }
        }  // conv
        public Command Command
        {
            get
            {
                return (Command)Buffer.Span[4];
            }
            set
            {
                Buffer.Span[4] = (byte)value;
            }
        }  // cmd
        public byte FragmentCount
        {
            get
            {
                return Buffer.Span[5];
            }
            set
            {
                Buffer.Span[5] = value;
            }
        }  // frg
        public ushort WindowSize
        {
            get
            {
                return BitConverter.ToUInt16(Buffer.Span[6..8]);
            }
            set
            {
                var segment = Buffer.Span[6..8];
                segment[0] = (byte)(value & 0xff);
                segment[1] = (byte)((value >> 8) & 0xff);
            }
        }  // wnd
        public uint Timestamp
        {
            get
            {
                return BitConverter.ToUInt32(Buffer.Span[8..12]);
            }
            set
            {
                var segment = Buffer.Span[8..12];
                segment[0] = (byte)(value & 0xff);
                segment[1] = (byte)((value >> 8) & 0xff);
                segment[2] = (byte)((value >> 16) & 0xff);
                segment[3] = (byte)((value >> 24) & 0xff);
            }
        }  // ts
        public uint SequenceNumber
        {
            get
            {
                return BitConverter.ToUInt32(Buffer.Span[12..16]);
            }
            set
            {
                var segment = Buffer.Span[12..16];
                segment[0] = (byte)(value & 0xff);
                segment[1] = (byte)((value >> 8) & 0xff);
                segment[2] = (byte)((value >> 16) & 0xff);
                segment[3] = (byte)((value >> 24) & 0xff);
            }
        }  // sn
        public uint UnacknowledgedNumber
        {
            get
            {
                return BitConverter.ToUInt32(Buffer.Span[16..20]);
            }
            set
            {
                var segment = Buffer.Span[16..20];
                segment[0] = (byte)(value & 0xff);
                segment[1] = (byte)((value >> 8) & 0xff);
                segment[2] = (byte)((value >> 16) & 0xff);
                segment[3] = (byte)((value >> 24) & 0xff);
            }
        }  // una
        public uint DataLength
        {
            get
            {
                return BitConverter.ToUInt32(Buffer.Span[20..24]);
            }
            set
            {
                var segment = Buffer.Span[20..24];
                segment[0] = (byte)(value & 0xff);
                segment[1] = (byte)((value >> 8) & 0xff);
                segment[2] = (byte)((value >> 16) & 0xff);
                segment[3] = (byte)((value >> 24) & 0xff);
            }
        }  // len
        public Span<byte> Data
        {
            get
            {
                return Buffer.Span[(int)DataOffset..];
            }
        }

        // header + data
        public int RawSegmentLength { get => (int)DataOffset + (int)DataLength; }
        public Span<byte> RawSegment
        {
            get
            {
                return Buffer.Span[..RawSegmentLength];
            }
        }

        // public KcpSegment(int bufferSize)
        public KcpSegment(uint dataLength)
        {
            // Buffer = new byte[bufferSize];
            Buffer = new byte[dataLength + (int)KcpSegment.DataOffset];

            ConversationId = 0;
            Command = Command.Push;
            FragmentCount = 0;
            WindowSize = 0;
            Timestamp = 0;
            SequenceNumber = 0;
            UnacknowledgedNumber = 0;
            DataLength = 0;  // there is currently no data in the segment
        }

        public KcpSegment(byte[] buffer)
        {
            Buffer = buffer;
        }

        public KcpSegment(Memory<byte> buffer)
        {
            Buffer = buffer;
        }

        public KcpSegment(Span<byte> buffer)
        {
            this.Buffer = new byte[buffer.Length];
            buffer.CopyTo(this.Buffer.Span);
        }

        // return the amount of data being appended to this segment
        public int Append(Span<byte> newData)
        {
            var freeSpace = this.Data[(int)this.DataLength..];
            int numBytesToAppend = Math.Min(freeSpace.Length, newData.Length);
            newData[0..numBytesToAppend].CopyTo(freeSpace);
            this.DataLength += (uint)numBytesToAppend;
            return numBytesToAppend;
        }
    }
}
