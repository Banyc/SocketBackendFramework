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
        public event EventHandler<int>? ReceivedCompleteSegment;

        public KcpControl(KcpConfig config)  // onOutput(byte[] data, int length)
        {
            this.conversationId = config.ConversationId;
            this.IsStreamMode = config.IsStreamMode;
            this.receiveWindowSize = config.ReceiveWindowSize;
            this.isNoDelayAck = config.IsNoDelayAck;
            if (config.RetransmissionTimeout != null)
            {
                this.RetransmissionTimeout = config.RetransmissionTimeout.Value;
            }
            else
            {
                this.RetransmissionTimeout = TimeSpan.FromSeconds(3);
            }

            if (config.OutputDuration != null)
            {
                this.transmissionTimer = new Timer(config.OutputDuration.Value.TotalMilliseconds);
            }
            else
            {
                this.transmissionTimer = new Timer(10);
            }
            this.transmissionTimer.AutoReset = false;
            this.transmissionTimer.Elapsed += (sender, e) =>
            {
                this.TryOutput();
                // the timer could have already been disposed before the lock was acquired
                this.transmissionTimer?.Start();
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

        private bool isTryingOutput = false;
        private readonly object tryOutputLock = new();
        private void TryOutput()
        {
            // make sure it's only one thread calling event this.TryingOutput
            lock (this.tryOutputLock)
            {
                if (this.isTryingOutput)
                {
                    return;
                }
                this.isTryingOutput = true;
            }

            // the output order is guaranteed since there is only one thread calling this event
            this.TryingOutput?.Invoke(this, EventArgs.Empty);
            this.isTryingOutput = false;
        }
    }
}
