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
            KcpControl kcpControl_1 = new KcpControl(0x1, false);
            KcpControl kcpControl_2 = new KcpControl(0x1, false);

            string applicationString = "hello world";
            byte[] applicationBytes = System.Text.Encoding.UTF8.GetBytes(applicationString);

            // send application bytes
            kcpControl_1.Send(applicationBytes);
            writtenDataSize = kcpControl_1.Output(bufferSpan);
            kcpControl_2.Input(bufferSpan[..writtenDataSize]);

            // ack
            writtenDataSize = kcpControl_2.Output(bufferSpan);
            Assert.Equal((int)KcpSegment.DataOffset, writtenDataSize);
            kcpControl_1.Input(bufferSpan[..writtenDataSize]);
            
            // get application bytes
            byte[] receivedApplicationBytes = new byte[1024 * 1024 * 10];
            int receivedApplicationByteSize = kcpControl_2.Receive(receivedApplicationBytes);
            string receivedApplicationString = System.Text.Encoding.UTF8.GetString(receivedApplicationBytes.AsSpan()[..receivedApplicationByteSize]);

            Assert.Equal(applicationString, receivedApplicationString);
        }
    }
}
