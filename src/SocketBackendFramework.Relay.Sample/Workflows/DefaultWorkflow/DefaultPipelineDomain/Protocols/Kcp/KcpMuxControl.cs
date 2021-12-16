using System;
using System.Collections.Generic;
// using System.Timers;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp
{
    public partial class KcpMuxControl : IDisposable
    {
        private readonly Dictionary<uint, KcpControl> kcpControls = new();

        // private readonly Timer? outputTimer;
        private readonly KcpConfig config;
        private readonly uint baseConversationId;

        public event EventHandler? TryingOutput;

        public string Name { get; set; }

        private readonly UniqueQueue<KcpControl> pendingOutputRequests = new();

        public KcpMuxControl(KcpConfig config, string name)  // onOutput(byte[] data, int length)
        {
            this.config = config;
            baseConversationId = config.ConversationId;
            this.Name = name;

            // if (config.OutputDuration != null)
            // {
            //     this.outputTimer = new Timer(config.OutputDuration.Value.TotalMilliseconds);
            // }
            // else
            // {
            //     this.outputTimer = new Timer(10);
            // }
            // this.outputTimer.AutoReset = false;
            // this.outputTimer.Elapsed += (sender, e) =>
            // {
            //     this.TryOutput();
            //     // the timer could have already been disposed before the lock was acquired
            //     this.outputTimer?.Start();
            // };

            // this.outputTimer?.Start();
        }

        public void Dispose()
        {
            System.Diagnostics.Debug.WriteLine("Disposing KcpMuxControl");
            lock (this.kcpControls)
            {
                // this.outputTimer?.Stop();
                // this.outputTimer?.Dispose();
                // this.outputTimer = null;
                foreach (var (_, kcpControl) in this.kcpControls)
                {
                    kcpControl.Dispose();
                }
                this.kcpControls.Clear();
            }
            GC.SuppressFinalize(this);
        }

        public KcpControl NewKcpControl()
        {
            KcpConfig config = (KcpConfig)this.config.Clone();
            // config.OutputDuration = null;  // stop the outputTimer in the new KcpControl
            KcpControl kcpControl;
            lock (this.kcpControls)
            {
                config.ConversationId = this.baseConversationId + (uint)this.kcpControls.Count;
                kcpControl = new(config);

                kcpControl.TryingOutput += (sender, e) =>
                {
                    if (this.TryingOutput == null)
                    {
                        return;
                    }
                    lock (this.pendingOutputRequests)
                    {
                        this.pendingOutputRequests.Enqueue(kcpControl);
                    }
                    this.TryOutput();
                };

                this.kcpControls[config.ConversationId] = kcpControl;
            }
            return kcpControl;
        }

        private bool isTryingOutput = false;
        private readonly object tryOutputLock = new();
        // thread-safe
        // ask for external event handlers of this.TryingOutput calling this.Output()
        // do NOT call this.TryOutput() within a locked scope
        private void TryOutput()
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

            while (true)
            {
                // the output order is guaranteed since there is only one thread calling this event
                this.TryingOutput?.Invoke(this, EventArgs.Empty);

                lock (this.pendingOutputRequests)
                {
                    // make sure all requests are processed
                    if (this.pendingOutputRequests.Count == 0)
                    {
                        this.isTryingOutput = false;
                        return;
                    }
                }
            }
        }
    }
}
