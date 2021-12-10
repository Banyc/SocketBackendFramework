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
            KcpConfig config = new()
            {
                ConversationId = (uint)Guid.NewGuid().GetHashCode(),
                IsStreamMode = false,
                ReceiveWindowSize = 0,
                ShouldSendSmallPacketsNoDelay = false,
                RetransmissionTimeout = TimeSpan.FromSeconds(3),
                OutputDuration = TimeSpan.FromMilliseconds(10),
            };
            using KcpControl kcpControl_1 = new KcpControl(config);
            using KcpControl kcpControl_2 = new KcpControl(config);

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
        public async Task Test1WithEventHandlingAsync()
        {
            KcpConfig config = new()
            {
                ConversationId = (uint)Guid.NewGuid().GetHashCode(),
                IsStreamMode = false,
                ReceiveWindowSize = 0,
                ShouldSendSmallPacketsNoDelay = false,
                RetransmissionTimeout = TimeSpan.FromSeconds(3),
                OutputDuration = TimeSpan.FromMilliseconds(10),
            };
            using KcpControl kcpControl_1 = new KcpControl(config);
            using KcpControl kcpControl_2 = new KcpControl(config);

            kcpControl_1.TryingOutput += (sender, e) =>
            {
                Console.WriteLine($"kcpControl_1.TryingOutput");
                while (true)
                {
                    byte[] bigBuffer = new byte[kcpControl_1.Mtu];
                    Span<byte> bufferSpan = bigBuffer.AsSpan();
                    int writtenDataSize = kcpControl_1.Output(bufferSpan);
                    if (writtenDataSize <= 0)
                    {
                        break;
                    }
                    // Task.Run(() =>
                    // {
                        System.Diagnostics.Debug.WriteLine($"kcpControl_1.TryingOutput: {writtenDataSize} bytes");
                        kcpControl_2.Input(bigBuffer.AsSpan()[..writtenDataSize]);
                    // });
                }
            };
            kcpControl_2.TryingOutput += (sender, e) =>
            {
                Console.WriteLine($"kcpControl_2.TryingOutput");
                while (true)
                {
                    byte[] bigBuffer = new byte[kcpControl_2.Mtu];
                    Span<byte> bufferSpan = bigBuffer.AsSpan();
                    int writtenDataSize = kcpControl_2.Output(bufferSpan);
                    if (writtenDataSize <= 0)
                    {
                        break;
                    }
                    // Task.Run(() =>
                    // {
                        System.Diagnostics.Debug.WriteLine($"kcpControl_2.TryingOutput: {writtenDataSize} bytes");
                        kcpControl_1.Input(bigBuffer.AsSpan()[..writtenDataSize]);
                    // });
                }
            };

            string applicationString = "hello world";
            byte[] applicationBytes = System.Text.Encoding.UTF8.GetBytes(applicationString);

            // kcpControl_1 sends application bytes
            kcpControl_1.Send(applicationBytes);

            await Task.Delay(100);

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
            KcpConfig config = new()
            {
                ConversationId = (uint)Guid.NewGuid().GetHashCode(),
                IsStreamMode = false,
                ReceiveWindowSize = 3,
                ShouldSendSmallPacketsNoDelay = false,
                RetransmissionTimeout = TimeSpan.FromSeconds(3),
                OutputDuration = TimeSpan.FromMilliseconds(10),
            };
            using KcpControl kcpControl_1 = new KcpControl(config);
            using KcpControl kcpControl_2 = new KcpControl(config);

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

        [Fact]
        public async Task ApplicationBytesAreABitLongWithEventHandlingAsync()
        {
            Random random = new Random();
            KcpConfig config = new()
            {
                ConversationId = (uint)Guid.NewGuid().GetHashCode(),
                IsStreamMode = false,
                ReceiveWindowSize = 3,
                ShouldSendSmallPacketsNoDelay = false,
                RetransmissionTimeout = TimeSpan.FromSeconds(3),
                OutputDuration = TimeSpan.FromMilliseconds(10),
            };
            using KcpControl kcpControl_1 = new KcpControl(config);
            using KcpControl kcpControl_2 = new KcpControl(config);

            kcpControl_1.TryingOutput += (sender, e) =>
            {
                Console.WriteLine($"kcpControl_1.TryingOutput");
                while (true)
                {
                    byte[] bigBuffer = new byte[kcpControl_1.Mtu];
                    Span<byte> bufferSpan = bigBuffer.AsSpan();
                    // must be declared in the loop so that the value in the new thread will not be altered
                    int writtenDataSize = kcpControl_1.Output(bufferSpan);
                    if (writtenDataSize <= 0)
                    {
                        break;
                    }
                    Task.Run(() =>
                    {
                        System.Diagnostics.Debug.WriteLine($"kcpControl_1.TryingOutput: {writtenDataSize} bytes");
                        kcpControl_2.Input(bigBuffer.AsSpan()[..writtenDataSize]);
                    });
                }
            };
            kcpControl_2.TryingOutput += (sender, e) =>
            {
                Console.WriteLine($"kcpControl_2.TryingOutput");
                while (true)
                {
                    byte[] bigBuffer = new byte[kcpControl_2.Mtu];
                    Span<byte> bufferSpan = bigBuffer.AsSpan();
                    int writtenDataSize = kcpControl_2.Output(bufferSpan);
                    if (writtenDataSize <= 0)
                    {
                        break;
                    }
                    Task.Run(() =>
                    {
                        System.Diagnostics.Debug.WriteLine($"kcpControl_2.TryingOutput: {writtenDataSize} bytes");
                        kcpControl_1.Input(bigBuffer.AsSpan()[..writtenDataSize]);
                    });
                }
            };

            // it requires kcpControl_1 to send three segments
            byte[] applicationBytes = new byte[kcpControl_1.Mtu * 2];
            random.NextBytes(applicationBytes);
            kcpControl_1.Send(applicationBytes);

            await Task.Delay(100);

            // kcpControl_2 gets application bytes
            byte[] receivedApplicationBytes = new byte[1024 * 1024 * 10];
            int receivedApplicationByteSize = kcpControl_2.Receive(receivedApplicationBytes);

            Assert.True(applicationBytes.AsSpan().SequenceEqual(receivedApplicationBytes.AsSpan()[..receivedApplicationByteSize]));
        }

        [Fact]
        public async Task ApplicationBytesAreABitLongWithMoreEventHandlingAsync()
        {
            Random random = new Random();
            KcpConfig config = new()
            {
                ConversationId = (uint)Guid.NewGuid().GetHashCode(),
                IsStreamMode = false,
                ReceiveWindowSize = 3,
                ShouldSendSmallPacketsNoDelay = false,
                RetransmissionTimeout = TimeSpan.FromMilliseconds(10),
                OutputDuration = TimeSpan.FromMilliseconds(10),
            };
            using KcpControl kcpControl_1 = new KcpControl(config);
            using KcpControl kcpControl_2 = new KcpControl(config);

            Queue<byte[]> applicationBytesQueue = new Queue<byte[]>();

            kcpControl_1.TryingOutput += (sender, e) =>
            {
                Console.WriteLine($"kcpControl_1.TryingOutput");
                while (true)
                {
                    byte[] bigBuffer = new byte[kcpControl_1.Mtu];
                    Span<byte> bufferSpan = bigBuffer.AsSpan();
                    int writtenDataSize = kcpControl_1.Output(bufferSpan);
                    if (writtenDataSize <= 0)
                    {
                        break;
                    }
                    // Task.Run(() =>
                    // {
                        System.Diagnostics.Debug.WriteLine($"kcpControl_1.TryingOutput: {writtenDataSize} bytes");
                        kcpControl_2.Input(bigBuffer.AsSpan()[..writtenDataSize]);
                    // });
                }
            };
            kcpControl_2.TryingOutput += (sender, e) =>
            {
                Console.WriteLine($"kcpControl_2.TryingOutput");
                while (true)
                {
                    byte[] bigBuffer = new byte[kcpControl_2.Mtu];
                    Span<byte> bufferSpan = bigBuffer.AsSpan();
                    int writtenDataSize = kcpControl_2.Output(bufferSpan);
                    if (writtenDataSize <= 0)
                    {
                        break;
                    }
                    // Task.Run(() =>
                    // {
                        System.Diagnostics.Debug.WriteLine($"kcpControl_2.TryingOutput: {writtenDataSize} bytes");
                        kcpControl_1.Input(bigBuffer.AsSpan()[..writtenDataSize]);
                    // });
                }
            };
            kcpControl_2.ReceivedCompleteSegment += (sender, completeSegmentBatchCount) =>
            {
                Console.WriteLine($"kcpControl_2.ReceivedCompleteSegment");
                int i;
                for (i = 0; i < completeSegmentBatchCount; i++)
                {
                    byte[] receivedApplicationBytes = new byte[1024 * 1024 * 10];
                    int receivedApplicationByteSize;
                    receivedApplicationByteSize = kcpControl_2.Receive(receivedApplicationBytes);
                    byte[] previousSent;
                    lock (applicationBytesQueue)
                    {
                        previousSent = applicationBytesQueue.Dequeue();
                    }
                    Assert.True(previousSent.AsSpan().SequenceEqual(receivedApplicationBytes.AsSpan()[..receivedApplicationByteSize]));
                }
            };

            // kcpControl_1 sends application bytes
            for (int i = 0; i < 30; i++)
            {
                byte[] applicationBytes = new byte[kcpControl_1.Mtu * 2];
                random.NextBytes(applicationBytes);
                // print first 6 bytes
                Console.WriteLine($"{i}: {applicationBytes[0]}, {applicationBytes[1]}, {applicationBytes[2]}, {applicationBytes[3]}, {applicationBytes[4]}, {applicationBytes[5]}");
                lock (applicationBytesQueue)
                {
                    applicationBytesQueue.Enqueue(applicationBytes);
                }
                kcpControl_1.Send(applicationBytes);
            }

            await Task.Delay(2000);

            lock (applicationBytesQueue)
            {
                Assert.True(applicationBytesQueue.Count == 0);
            }
        }

        [Fact]
        public async Task ApplicationBytesAreABitLongWithMoreEventHandlingAndPacketLossAsync()
        {
            Random random = new Random();
            KcpConfig config = new()
            {
                ConversationId = (uint)Guid.NewGuid().GetHashCode(),
                IsStreamMode = false,
                ReceiveWindowSize = 3,
                ShouldSendSmallPacketsNoDelay = false,
                RetransmissionTimeout = TimeSpan.FromMilliseconds(10),
                OutputDuration = TimeSpan.FromMilliseconds(10),
            };
            using KcpControl kcpControl_1 = new KcpControl(config);
            using KcpControl kcpControl_2 = new KcpControl(config);

            Queue<byte[]> applicationBytesQueue = new Queue<byte[]>();

            kcpControl_1.TryingOutput += (sender, e) =>
            {
                Console.WriteLine($"kcpControl_1.TryingOutput");
                while (true)
                {
                    byte[] bigBuffer = new byte[kcpControl_1.Mtu];
                    Span<byte> bufferSpan = bigBuffer.AsSpan();
                    // must be declared in the loop so that the value in the new thread will not be altered
                    int writtenDataSize = kcpControl_1.Output(bufferSpan);
                    if (writtenDataSize <= 0)
                    {
                        break;
                    }
                    if (random.Next(0, 100) < 10)
                    {
                        // oops, packet loss
                        Console.WriteLine($"kcpControl_1.TryingOutput: {writtenDataSize} bytes lost");
                        continue;
                    }
                    System.Diagnostics.Debug.WriteLine($"kcpControl_1.TryingOutput: {writtenDataSize} bytes");
                    kcpControl_2.Input(bigBuffer.AsSpan()[..writtenDataSize]);
                }
            };
            kcpControl_2.TryingOutput += (sender, e) =>
            {
                Console.WriteLine($"kcpControl_2.TryingOutput");
                while (true)
                {
                    byte[] bigBuffer = new byte[kcpControl_2.Mtu];
                    Span<byte> bufferSpan = bigBuffer.AsSpan();
                    int writtenDataSize = kcpControl_2.Output(bufferSpan);
                    if (writtenDataSize <= 0)
                    {
                        break;
                    }
                    if (random.Next(0, 100) < 10)
                    {
                        // oops, packet loss
                        Console.WriteLine($"kcpControl_2.TryingOutput: {writtenDataSize} bytes lost");
                        continue;
                    }
                    System.Diagnostics.Debug.WriteLine($"kcpControl_2.TryingOutput: {writtenDataSize} bytes");
                    kcpControl_1.Input(bigBuffer.AsSpan()[..writtenDataSize]);
                }
            };
            kcpControl_2.ReceivedCompleteSegment += (sender, completeSegmentBatchCount) =>
            {
                Console.WriteLine($"kcpControl_2.ReceivedCompleteSegment");
                int i;
                for (i = 0; i < completeSegmentBatchCount; i++)
                {
                    byte[] receivedApplicationBytes = new byte[1024 * 1024 * 10];
                    int receivedApplicationByteSize;
                    receivedApplicationByteSize = kcpControl_2.Receive(receivedApplicationBytes);
                    byte[] previousSent;
                    lock (applicationBytesQueue)
                    {
                        previousSent = applicationBytesQueue.Dequeue();
                    }
                    Assert.True(previousSent.AsSpan().SequenceEqual(receivedApplicationBytes.AsSpan()[..receivedApplicationByteSize]));
                }
            };

            // kcpControl_1 sends application bytes
            for (int i = 0; i < 300; i++)
            {
                byte[] applicationBytes = new byte[kcpControl_1.Mtu * 2];
                random.NextBytes(applicationBytes);
                lock (applicationBytesQueue)
                {
                    applicationBytesQueue.Enqueue(applicationBytes);
                }
                kcpControl_1.Send(applicationBytes);
            }

            await Task.Delay(20000);

            lock (applicationBytesQueue)
            {
                Assert.True(applicationBytesQueue.Count == 0);
            }
        }
    }
}
