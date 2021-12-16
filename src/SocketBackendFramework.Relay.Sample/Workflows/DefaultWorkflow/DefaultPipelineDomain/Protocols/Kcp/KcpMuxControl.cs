using System;
using System.Collections.Generic;
// using System.Timers;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp
{
    public partial class KcpMuxControl : IDisposable
    {
        private readonly Dictionary<uint, KcpControl> kcpControls = new();

        // private Timer? transmissionTimer;
        private KcpConfig config;
        private uint baseConversationId;

        public event EventHandler? TryingOutput;

        public string Name { get; set; }

        public KcpMuxControl(KcpConfig config, string name)  // onOutput(byte[] data, int length)
        {
            this.config = config;
            baseConversationId = config.ConversationId;
            this.Name = name;

            // if (config.OutputDuration != null)
            // {
            //     this.transmissionTimer = new Timer(config.OutputDuration.Value.TotalMilliseconds);
            // }
            // else
            // {
            //     this.transmissionTimer = new Timer(10);
            // }
            // this.transmissionTimer.AutoReset = false;
            // this.transmissionTimer.Elapsed += (sender, e) =>
            // {
            //     this.TryOutput();
            //     // the timer could have already been disposed before the lock was acquired
            //     this.transmissionTimer?.Start();
            // };

            // this.transmissionTimer?.Start();
        }

        public void Dispose()
        {
            System.Diagnostics.Debug.WriteLine("Disposing KcpMuxControl");
            lock (this.kcpControls)
            {
                // this.transmissionTimer?.Stop();
                // this.transmissionTimer?.Dispose();
                // this.transmissionTimer = null;
                foreach (var (_, kcpControl) in this.kcpControls)
                {
                    kcpControl.Dispose();
                }
                this.kcpControls.Clear();
            }
            GC.SuppressFinalize(this);
        }

        public (uint, KcpControl) AddKcpControl()
        {
            KcpConfig config = (KcpConfig)this.config.Clone();
            KcpControl kcpControl;
            lock (this.kcpControls)
            {
                config.ConversationId = this.baseConversationId + (uint)this.kcpControls.Count;
                kcpControl = new(config);

                kcpControl.TryingOutput += (sender, e) =>
                {
                    this.TryOutput();
                };

                this.kcpControls[config.ConversationId] = kcpControl;
            }
            return (config.ConversationId, kcpControl);
        }

        private bool isTryingOutput = false;
        private readonly object tryOutputLock = new();
        // thread-safe
        // ask for external event handlers of this.TryingOutput calling this.Output()
        // do NOT call this.TryOutput() within a locked scope
        private void TryOutput()
        {
            // to make sure it's only one thread calling event this.TryingOutput
            lock (this.tryOutputLock)  // protect this.isTryingOutput
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
