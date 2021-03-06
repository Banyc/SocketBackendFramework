using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public uint ConversationId { get; }
        private readonly bool shouldSendSmallPacketsNoDelay;
        private readonly uint receiveWindowSize;  // rcv_wnd  // out-of-order queue size
        private uint remoteWindowSize = 0;

        private static uint CurrentTimestamp { get => (uint)(DateTime.Now.ToBinary() >> 32); }  // current

        // sequence numbers to ack
        private readonly LinkedList<SequenceNumberTimestampPair> pendingAckList = new();  // ack list

        // sent but unacked segments
        private readonly KcpSegmentQueue sendingQueue = new();  // snd_buf

        // acked but out-of-order segments
        private readonly SortedDictionary<uint, KcpSegment> outOfOrderQueue = new();  // rcv_buf

        private Timer? outputTimer;
        private bool isStoppingOutputTimer = false;

        public event EventHandler? TryingOutput;
        public event EventHandler<int>? ReceivedCompleteSegment;

        public KcpControl(KcpConfig config)  // onOutput(byte[] data, int length)
        {
            this.ConversationId = config.ConversationId;
            this.IsStreamMode = config.IsStreamMode;
            this.receiveWindowSize = config.ReceiveWindowSize;
            this.shouldSendSmallPacketsNoDelay = config.ShouldSendSmallPacketsNoDelay;
            if (config.RetransmissionTimeout != null)
            {
                this.RetransmissionTimeout = config.RetransmissionTimeout.Value;
            }
            else
            {
                this.RetransmissionTimeout = TimeSpan.FromSeconds(3);
            }

            if (config.OutputDuration == null)
            {
                this.outputTimer = null;
            }
            else
            {
                this.outputTimer = new Timer(config.OutputDuration.Value.TotalMilliseconds)
                {
                    AutoReset = false
                };
                this.outputTimer.Elapsed += async (sender, e) =>
                {
                    await this.TryOutputAsync(shouldStartNewTask: false);

                    if (!this.isStoppingOutputTimer)
                    {
                        // the timer could have already been disposed before the lock was acquired
                        this.outputTimer?.Start();
                    }
                    // else conserve timer threads
                };

                this.outputTimer?.Start();
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                this.outputTimer?.Stop();
                this.outputTimer?.Dispose();
                this.outputTimer = null;
            }
            GC.SuppressFinalize(this);
        }

        private bool isTryingOutput = false;
        private readonly object tryOutputLock = new();
        // thread-safe
        // ask for external event handlers of this.TryingOutput calling this.Output()
        // do NOT call this.TryOutput() within a locked scope
        // param: shouldStartNewTask: set true except for calling from the timer event
        private async Task TryOutputAsync(bool shouldStartNewTask)
        {
            // to make sure it's only one thread calling event this.TryingOutput
            if (this.isTryingOutput)
            {
                return;
            }
            lock (this.tryOutputLock)  // protect this.isTryingOutput
            {
                if (this.isTryingOutput)
                {
                    return;
                }
                this.isTryingOutput = true;
            }

            // the output order is guaranteed since there is only one thread calling this event
            if (shouldStartNewTask)
            {
                await Task.Run(() => this.TryingOutput?.Invoke(this, EventArgs.Empty));
            }
            else
            {
                this.TryingOutput?.Invoke(this, EventArgs.Empty);
            }
            this.isTryingOutput = false;
        }

        private void StartOutputTimer()
        {
            this.isStoppingOutputTimer = false;
            this.outputTimer?.Start();
        }
    }
}
