using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models;
using Xunit;

namespace SocketBackendFramework.Relay.Sample.Tests
{
    public class KcpMuxControlTests
    {
        private struct kcpControlInfo
        {
            public uint ConversationId { get; set; }
            public KcpControl Control { get; set; }
            public Queue<byte[]> SentBytes { get; set; }
            public TaskCompletionSource<object?> ReceiveTask { get; set; }
        }

        // - ApplicationBytesAreABitLong
        // - MoreEventHandling
        // - PacketLoss
        // - GoBothWays
        // - Inactive KcpMuxControl
        [Fact]
        public async Task InactiveKcpMuxControlTest()
        {
            // hyperparameters
            int pathCount = 10;
            int numTx = 300;
            TimeSpan timeout = TimeSpan.FromSeconds(40);

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
            using KcpMuxControl kcpMuxControl1 = new(config, "1");
            using KcpMuxControl kcpMuxControl2 = new(config, "2");
            Dictionary<uint, kcpControlInfo> kcpControls1 = new();
            Dictionary<uint, kcpControlInfo> kcpControls2 = new();

            for (int i = 0; i < pathCount; i++)
            {
                (uint conversationId1, KcpControl kcpControl1) = kcpMuxControl1.AddKcpControl();
                (uint conversationId2, KcpControl kcpControl2) = kcpMuxControl2.AddKcpControl();
                Queue<byte[]> sentBytes1 = new();
                Queue<byte[]> sentBytes2 = new();
                TaskCreationOptions taskCreationOptions = TaskCreationOptions.RunContinuationsAsynchronously;
                TaskCompletionSource<object?> receiveTask1 = new(taskCreationOptions);
                TaskCompletionSource<object?> receiveTask2 = new(taskCreationOptions);
                kcpControls1.Add(conversationId1, new()
                {
                    ConversationId = conversationId1,
                    Control = kcpControl1,
                    SentBytes = sentBytes1,
                    ReceiveTask = receiveTask1
                });
                kcpControls2.Add(conversationId2, new()
                {
                    ConversationId = conversationId2,
                    Control = kcpControl2,
                    SentBytes = sentBytes2,
                    ReceiveTask = receiveTask2
                });

                kcpControl1.TryingOutput += (sender, e) =>
                {
                    Console.WriteLine($"kcpControl1.TryingOutput");
                    while (true)
                    {
                        byte[] bigBuffer = new byte[1024 * 1024 * 10];
                        Span<byte> bufferSpan = bigBuffer.AsSpan();
                        // must be declared in the loop so that the value in the new thread will not be altered
                        int writtenDataSize = kcpControl1.Output(bufferSpan);
                        if (writtenDataSize <= 0)
                        {
                            break;
                        }
                        if (random.Next(0, 100) < 10)
                        {
                            // oops, packet loss
                            Console.WriteLine($"kcpControl1.TryingOutput: {writtenDataSize} bytes lost");
                            continue;
                        }
                        System.Diagnostics.Debug.WriteLine($"kcpControl1.TryingOutput: {writtenDataSize} bytes");
                        kcpControl2.Input(bigBuffer.AsSpan()[..writtenDataSize]);
                    }
                };
                kcpControl2.TryingOutput += (sender, e) =>
                {
                    Console.WriteLine($"kcpControl2.TryingOutput");
                    while (true)
                    {
                        byte[] bigBuffer = new byte[1024 * 1024 * 10];
                        Span<byte> bufferSpan = bigBuffer.AsSpan();
                        int writtenDataSize = kcpControl2.Output(bufferSpan);
                        if (writtenDataSize <= 0)
                        {
                            break;
                        }
                        if (random.Next(0, 100) < 10)
                        {
                            // oops, packet loss
                            Console.WriteLine($"kcpControl2.TryingOutput: {writtenDataSize} bytes lost");
                            continue;
                        }
                        System.Diagnostics.Debug.WriteLine($"kcpControl2.TryingOutput: {writtenDataSize} bytes");
                        kcpControl1.Input(bigBuffer.AsSpan()[..writtenDataSize]);
                    }
                };
                kcpControl1.ReceivedCompleteSegment += (sender, completeSegmentBatchCount) =>
                {
                    Console.WriteLine($"kcpControl1.ReceivedCompleteSegment");
                    int i;
                    for (i = 0; i < completeSegmentBatchCount; i++)
                    {
                        byte[] receivedApplicationBytes = new byte[1024 * 1024 * 10];
                        int receivedApplicationByteSize;
                        receivedApplicationByteSize = kcpControl1.Receive(receivedApplicationBytes);
                        byte[] previousSent;
                        bool isApplicationBytesQueue2Empty = false;
                        lock (sentBytes2)
                        {
                            previousSent = sentBytes2.Dequeue();
                            isApplicationBytesQueue2Empty = sentBytes2.Count == 0;
                        }
                        try
                        {
                            Assert.True(previousSent.AsSpan().SequenceEqual(receivedApplicationBytes.AsSpan()[..receivedApplicationByteSize]));
                        }
                        catch (Exception e)
                        {
                            receiveTask1.SetException(e);
                            throw;
                        }
                        if (isApplicationBytesQueue2Empty)
                        {
                            receiveTask1.SetResult(null);
                        }
                    }
                };
                kcpControl2.ReceivedCompleteSegment += (sender, completeSegmentBatchCount) =>
                {
                    Console.WriteLine($"kcpControl2.ReceivedCompleteSegment");
                    int i;
                    for (i = 0; i < completeSegmentBatchCount; i++)
                    {
                        byte[] receivedApplicationBytes = new byte[1024 * 1024 * 10];
                        int receivedApplicationByteSize;
                        receivedApplicationByteSize = kcpControl2.Receive(receivedApplicationBytes);
                        byte[] previousSent;
                        bool isApplicationBytesQueue1Empty = false;
                        lock (sentBytes1)
                        {
                            previousSent = sentBytes1.Dequeue();
                            isApplicationBytesQueue1Empty = sentBytes1.Count == 0;
                        }
                        try
                        {
                            Assert.True(previousSent.AsSpan().SequenceEqual(receivedApplicationBytes.AsSpan()[..receivedApplicationByteSize]));
                        }
                        catch (Exception e)
                        {
                            receiveTask2.SetException(e);
                            throw;
                        }
                        if (isApplicationBytesQueue1Empty)
                        {
                            receiveTask2.SetResult(null);
                        }
                    }
                };
            }

            // activate stopwatch
            Stopwatch stopwatch = Stopwatch.StartNew();

            // kcpControl1 sends application bytes
            foreach ((uint conversationIdKey, kcpControlInfo info) in kcpControls1)
            {
                _ = Task.Run(() =>
                {
                    for (int i = 0; i < numTx; i++)
                    {
                        byte[] applicationBytes = new byte[info.Control.Mtu * 2];
                        random.NextBytes(applicationBytes);
                        lock (info.SentBytes)
                        {
                            info.SentBytes.Enqueue(applicationBytes);
                        }
                    }
                    List<byte[]> sentBytes = new();
                    lock (info.SentBytes)
                    {
                        sentBytes.AddRange(info.SentBytes);
                    }
                    foreach (byte[] applicationBytes in sentBytes)
                    {
                        info.Control.Send(applicationBytes);
                    }
                });
            }
            // kcpControl_2 sends application bytes
            foreach ((uint conversationIdKey, kcpControlInfo info) in kcpControls2)
            {
                _ = Task.Run(() =>
                {
                    for (int i = 0; i < numTx; i++)
                    {
                        byte[] applicationBytes = new byte[info.Control.Mtu * 2];
                        random.NextBytes(applicationBytes);
                        lock (info.SentBytes)
                        {
                            info.SentBytes.Enqueue(applicationBytes);
                        }
                    }
                    List<byte[]> sentBytes = new();
                    lock (info.SentBytes)
                    {
                        sentBytes.AddRange(info.SentBytes);
                    }
                    foreach (byte[] applicationBytes in sentBytes)
                    {
                        info.Control.Send(applicationBytes);
                    }
                });
            }

            Task timeoutTask = Task.Delay(timeout);

            List<Task> testTasks = new();
            testTasks.AddRange(kcpControls1.Select(keyValuePair => keyValuePair.Value.ReceiveTask.Task));
            testTasks.AddRange(kcpControls2.Select(keyValuePair => keyValuePair.Value.ReceiveTask.Task));
            Task testTask = Task.WhenAll(testTasks);
            await Task.WhenAny(testTask, timeoutTask);

            if (!testTask.IsCompleted)
            {
                throw new TimeoutException();
            }

            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"{stopwatch.ElapsedMilliseconds} ms");

            foreach ((uint conversationIdKey, kcpControlInfo info) in kcpControls1)
            {
                lock (info.SentBytes)
                {
                    Assert.True(info.SentBytes.Count == 0);
                }
            }
            foreach ((uint conversationIdKey, kcpControlInfo info) in kcpControls2)
            {
                lock (info.SentBytes)
                {
                    Assert.True(info.SentBytes.Count == 0);
                }
            }
        }

        // - ApplicationBytesAreABitLong
        // - MoreEventHandling
        // - PacketLoss
        // - GoBothWays
        // - Active KcpMuxControl
        [Fact]
        public async Task ActiveKcpMuxControlTest()
        {
            // hyperparameters
            int pathCount = 10;
            int numTx = 30;
            TimeSpan timeout = TimeSpan.FromSeconds(60);

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
            using KcpMuxControl kcpMuxControl1 = new(config, "1");
            using KcpMuxControl kcpMuxControl2 = new(config, "2");
            Dictionary<uint, kcpControlInfo> kcpControls1 = new();
            Dictionary<uint, kcpControlInfo> kcpControls2 = new();

            for (int i = 0; i < pathCount; i++)
            {
                (uint conversationId1, KcpControl kcpControl1) = kcpMuxControl1.AddKcpControl();
                (uint conversationId2, KcpControl kcpControl2) = kcpMuxControl2.AddKcpControl();
                Queue<byte[]> sentBytes1 = new();
                Queue<byte[]> sentBytes2 = new();
                TaskCreationOptions taskCreationOptions = TaskCreationOptions.RunContinuationsAsynchronously;
                TaskCompletionSource<object?> receiveTask1 = new(taskCreationOptions);
                TaskCompletionSource<object?> receiveTask2 = new(taskCreationOptions);
                kcpControls1.Add(conversationId1, new()
                {
                    ConversationId = conversationId1,
                    Control = kcpControl1,
                    SentBytes = sentBytes1,
                    ReceiveTask = receiveTask1
                });
                kcpControls2.Add(conversationId2, new()
                {
                    ConversationId = conversationId2,
                    Control = kcpControl2,
                    SentBytes = sentBytes2,
                    ReceiveTask = receiveTask2
                });

                kcpControl1.ReceivedCompleteSegment += (sender, completeSegmentBatchCount) =>
                {
                    Console.WriteLine($"kcpControl1.ReceivedCompleteSegment");
                    int i;
                    for (i = 0; i < completeSegmentBatchCount; i++)
                    {
                        byte[] receivedApplicationBytes = new byte[1024 * 1024 * 10];
                        int receivedApplicationByteSize;
                        receivedApplicationByteSize = kcpControl1.Receive(receivedApplicationBytes);
                        byte[] previousSent;
                        bool isApplicationBytesQueue2Empty = false;
                        lock (sentBytes2)
                        {
                            previousSent = sentBytes2.Dequeue();
                            isApplicationBytesQueue2Empty = sentBytes2.Count == 0;
                        }
                        try
                        {
                            Assert.True(previousSent.AsSpan().SequenceEqual(receivedApplicationBytes.AsSpan()[..receivedApplicationByteSize]));
                        }
                        catch (Exception e)
                        {
                            receiveTask1.SetException(e);
                            throw;
                        }
                        if (isApplicationBytesQueue2Empty)
                        {
                            receiveTask1.SetResult(null);
                        }
                    }
                };
                kcpControl2.ReceivedCompleteSegment += (sender, completeSegmentBatchCount) =>
                {
                    Console.WriteLine($"kcpControl2.ReceivedCompleteSegment");
                    int i;
                    for (i = 0; i < completeSegmentBatchCount; i++)
                    {
                        byte[] receivedApplicationBytes = new byte[1024 * 1024 * 10];
                        int receivedApplicationByteSize;
                        receivedApplicationByteSize = kcpControl2.Receive(receivedApplicationBytes);
                        byte[] previousSent;
                        bool isApplicationBytesQueue1Empty = false;
                        lock (sentBytes1)
                        {
                            previousSent = sentBytes1.Dequeue();
                            isApplicationBytesQueue1Empty = sentBytes1.Count == 0;
                        }
                        try
                        {
                            Assert.True(previousSent.AsSpan().SequenceEqual(receivedApplicationBytes.AsSpan()[..receivedApplicationByteSize]));
                        }
                        catch (Exception e)
                        {
                            receiveTask2.SetException(e);
                            throw;
                        }
                        if (isApplicationBytesQueue1Empty)
                        {
                            receiveTask2.SetResult(null);
                        }
                    }
                };
            }
            kcpMuxControl1.TryingOutput += (sender, e) =>
            {
                Console.WriteLine($"kcpMuxControl1.TryingOutput");
                while (true)
                {
                    byte[] bigBuffer = new byte[1024 * 1024 * 10];
                    Span<byte> bufferSpan = bigBuffer.AsSpan();
                    // must be declared in the loop so that the value in the new thread will not be altered
                    int writtenDataSize = kcpMuxControl1.Output(bufferSpan);
                    if (writtenDataSize <= 0)
                    {
                        break;
                    }
                    if (random.Next(0, 100) < 10)
                    {
                        // oops, packet loss
                        Console.WriteLine($"kcpMuxControl1.TryingOutput: {writtenDataSize} bytes lost");
                        continue;
                    }
                    System.Diagnostics.Debug.WriteLine($"kcpMuxControl1.TryingOutput: {writtenDataSize} bytes");
                    kcpMuxControl2.Input(bigBuffer.AsSpan()[..writtenDataSize]);
                }
            };
            kcpMuxControl2.TryingOutput += (sender, e) =>
            {
                Console.WriteLine($"kcpMuxControl2.TryingOutput");
                while (true)
                {
                    byte[] bigBuffer = new byte[1024 * 1024 * 10];
                    Span<byte> bufferSpan = bigBuffer.AsSpan();
                    // must be declared in the loop so that the value in the new thread will not be altered
                    int writtenDataSize = kcpMuxControl2.Output(bufferSpan);
                    if (writtenDataSize <= 0)
                    {
                        break;
                    }
                    if (random.Next(0, 100) < 10)
                    {
                        // oops, packet loss
                        Console.WriteLine($"kcpMuxControl2.TryingOutput: {writtenDataSize} bytes lost");
                        continue;
                    }
                    System.Diagnostics.Debug.WriteLine($"kcpMuxControl2.TryingOutput: {writtenDataSize} bytes");
                    kcpMuxControl1.Input(bigBuffer.AsSpan()[..writtenDataSize]);
                }
            };

            // activate stopwatch
            Stopwatch stopwatch = Stopwatch.StartNew();

            // kcpControl1 sends application bytes
            foreach ((uint conversationIdKey, kcpControlInfo info) in kcpControls1)
            {
                _ = Task.Run(() =>
                {
                    for (int i = 0; i < numTx; i++)
                    {
                        byte[] applicationBytes = new byte[info.Control.Mtu * 2];
                        random.NextBytes(applicationBytes);
                        lock (info.SentBytes)
                        {
                            info.SentBytes.Enqueue(applicationBytes);
                        }
                    }
                    List<byte[]> sentBytes = new();
                    lock (info.SentBytes)
                    {
                        sentBytes.AddRange(info.SentBytes);
                    }
                    foreach (byte[] applicationBytes in sentBytes)
                    {
                        info.Control.Send(applicationBytes);
                    }
                });
            }
            // kcpControl_2 sends application bytes
            foreach ((uint conversationIdKey, kcpControlInfo info) in kcpControls2)
            {
                _ = Task.Run(() =>
                {
                    for (int i = 0; i < numTx; i++)
                    {
                        byte[] applicationBytes = new byte[info.Control.Mtu * 2];
                        random.NextBytes(applicationBytes);
                        lock (info.SentBytes)
                        {
                            info.SentBytes.Enqueue(applicationBytes);
                        }
                    }
                    List<byte[]> sentBytes = new();
                    lock (info.SentBytes)
                    {
                        sentBytes.AddRange(info.SentBytes);
                    }
                    foreach (byte[] applicationBytes in sentBytes)
                    {
                        info.Control.Send(applicationBytes);
                    }
                });
            }

            Task timeoutTask = Task.Delay(timeout);
            // Task timeoutTask = Task.Delay(-1);

            List<Task> testTasks = new();
            testTasks.AddRange(kcpControls1.Select(keyValuePair => keyValuePair.Value.ReceiveTask.Task));
            testTasks.AddRange(kcpControls2.Select(keyValuePair => keyValuePair.Value.ReceiveTask.Task));
            Task testTask = Task.WhenAll(testTasks);
            await Task.WhenAny(testTask, timeoutTask);

            if (!testTask.IsCompleted)
            {
                throw new TimeoutException();
            }

            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"{stopwatch.ElapsedMilliseconds} ms");

            foreach ((uint conversationIdKey, kcpControlInfo info) in kcpControls1)
            {
                lock (info.SentBytes)
                {
                    Assert.True(info.SentBytes.Count == 0);
                }
            }
            foreach ((uint conversationIdKey, kcpControlInfo info) in kcpControls2)
            {
                lock (info.SentBytes)
                {
                    Assert.True(info.SentBytes.Count == 0);
                }
            }
        }
    }
}
