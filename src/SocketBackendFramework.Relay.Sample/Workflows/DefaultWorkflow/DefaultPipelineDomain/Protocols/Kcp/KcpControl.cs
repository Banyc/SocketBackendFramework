using System;
using System.Collections.Generic;
using System.Timers;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp
{
    public partial class KcpControl : IDisposable
    {
        private struct SequenceNumberTimestampPair
        {
            public uint SequenceNumber;
            public uint Timestamp;
        }

        private readonly uint conversationId;

        private readonly uint receiveWindowSize;  // rcv_wnd  // out-of-order queue size
        private uint remoteWindowSize = 0;

        private static uint CurrentTimestamp { get => (uint)(DateTime.Now.ToBinary() >> 32); }  // current

        // sequence numbers to ack
        private readonly LinkedList<SequenceNumberTimestampPair> pendingAckList = new();  // ack list

        private Timer? transmissionTimer;

        public event EventHandler? TryingOutput;
        public event EventHandler? ReceivedNewSegment;

        public KcpControl(uint conversationId,
                          bool isStreamMode,
                          uint receiveWindowSize,
                          bool isNoDelayAck,
                          TimeSpan? retransmissionTimeout = null,
                          TimeSpan? outputDuration = null,
                          Action<byte[], int>? outputCallback = null)  // onOutput(byte[] data, int length)
        {
            this.conversationId = conversationId;
            this.isStreamMode = isStreamMode;
            this.receiveWindowSize = receiveWindowSize;
            this.isNoDelayAck = isNoDelayAck;
            if (retransmissionTimeout != null)
            {
                this.RetransmissionTimeout = retransmissionTimeout.Value;
            }
            else
            {
                this.RetransmissionTimeout = TimeSpan.FromSeconds(3);
            }

            // output callback
            this.outputCallback = outputCallback;

            if (outputDuration != null)
            {
                this.transmissionTimer = new Timer(outputDuration.Value.TotalMilliseconds);
            }
            else
            {
                this.transmissionTimer = new Timer(10);
            }
            this.transmissionTimer.AutoReset = false;
            this.transmissionTimer.Elapsed += (sender, e) =>
            {
                lock (this)
                {
                    this.TryOutputAll();
                    // the timer could have already been disposed before the lock was acquired
                    this.transmissionTimer?.Start();
                }
            };

            this.transmissionTimer?.Start();
        }

        public void Dispose()
        {
            lock (this)
            {
                this.transmissionTimer?.Stop();
                this.transmissionTimer?.Dispose();
                this.transmissionTimer = null;
            }
            GC.SuppressFinalize(this);
        }

        private void TryOutputAll()
        {
            this.TryingOutput?.Invoke(this, EventArgs.Empty);
            if (this.outputCallback == null)
            {
                return;
            }
            int txDataSize;
            do
            {
                byte[] txData = new byte[this.Mtu];
                txDataSize = this.Output(txData);
                if (txDataSize > 0)
                {
                    this.outputCallback(txData, txDataSize);
                }
            } while (txDataSize > 0);
        }
    }
}
