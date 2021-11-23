using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models;
using Xunit;

namespace tests.SocketBackendFramework.Relay.Sample.Tests
{
    public class KcpControlTests
    {
        [Fact]
        public void Test1()
        {
            byte[] bigBuffer = new byte[1024 * 1024 * 10];
            int writtenDataSize;
            Span<byte> bufferSpan = bigBuffer.AsSpan();
            KcpControl kcpControl_1 = new KcpControl(0x1, isStreamMode: false, receiveWindowSize: 0);
            KcpControl kcpControl_2 = new KcpControl(0x1, isStreamMode: false, receiveWindowSize: 0);

            string applicationString = "hello world";
            byte[] applicationBytes = System.Text.Encoding.UTF8.GetBytes(applicationString);

            // kcpControl_1 sends application bytes
            kcpControl_1.Send(applicationBytes);
            writtenDataSize = kcpControl_1.Output(bufferSpan);
            kcpControl_2.Input(bufferSpan[..writtenDataSize]);

            // kcpControl_2 acks
            writtenDataSize = kcpControl_2.Output(bufferSpan);
            Assert.Equal((int)KcpSegment.DataOffset, writtenDataSize);
            kcpControl_1.Input(bufferSpan[..writtenDataSize]);

            // kcpControl_2 gets application bytes
            byte[] receivedApplicationBytes = new byte[1024 * 1024 * 10];
            int receivedApplicationByteSize = kcpControl_2.Receive(receivedApplicationBytes);
            string receivedApplicationString = System.Text.Encoding.UTF8.GetString(receivedApplicationBytes.AsSpan()[..receivedApplicationByteSize]);

            Assert.Equal(applicationString, receivedApplicationString);
        }

        [Fact]
        public void ApplicationBytesAreABitLong()
        {
            Random random = new Random();
            byte[] bigBuffer = new byte[1024 * 1024 * 10];
            int writtenDataSize;
            Span<byte> bufferSpan = bigBuffer.AsSpan();
            KcpControl kcpControl_1 = new KcpControl(0x1, isStreamMode: false, receiveWindowSize: 3);
            KcpControl kcpControl_2 = new KcpControl(0x1, isStreamMode: false, receiveWindowSize: 3);

            // it requires kcpControl_1 to send three segments
            byte[] applicationBytes = new byte[kcpControl_1.Mtu * 2];
            random.NextBytes(applicationBytes);

            // kcpControl_1 sends application bytes
            kcpControl_1.Send(applicationBytes);
            writtenDataSize = kcpControl_1.Output(bufferSpan);
            kcpControl_2.Input(bufferSpan[..writtenDataSize]);

            // kcpControl_2 acks
            // let kcpControl_2 update the window size for kcpControl_1 so that kcpControl_1 can send more data
            writtenDataSize = kcpControl_2.Output(bufferSpan);
            Assert.Equal((int)KcpSegment.DataOffset * 1, writtenDataSize);
            kcpControl_1.Input(bufferSpan[..writtenDataSize]);

            // kcpControl_1 sends the rest of the application bytes
            writtenDataSize = kcpControl_1.Output(bufferSpan);
            kcpControl_2.Input(bufferSpan[..writtenDataSize]);
            writtenDataSize = kcpControl_1.Output(bufferSpan);
            kcpControl_2.Input(bufferSpan[..writtenDataSize]);

            // kcpControl_2 acks
            writtenDataSize = kcpControl_2.Output(bufferSpan);
            Assert.Equal((int)KcpSegment.DataOffset * 2, writtenDataSize);
            kcpControl_1.Input(bufferSpan[..writtenDataSize]);

            // kcpControl_2 gets application bytes
            byte[] receivedApplicationBytes = new byte[1024 * 1024 * 10];
            int receivedApplicationByteSize = kcpControl_2.Receive(receivedApplicationBytes);

            Assert.True(applicationBytes.AsSpan().SequenceEqual(receivedApplicationBytes.AsSpan()[..receivedApplicationByteSize]));
        }
    }
}
