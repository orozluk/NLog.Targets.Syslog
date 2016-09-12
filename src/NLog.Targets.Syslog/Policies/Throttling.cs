using System;
using System.Threading;
using NLog.Common;

namespace NLog.Targets.Syslog.Policies
{
    public class Throttling
    {
        /// <summary>The number of log entries, waiting to be processed, that triggers throttling</summary>
        public int Limit { get; set; }

        /// <summary>The throttling strategy to employ</summary>
        public ThrottlingStrategy Strategy { get; set; }

        /// <summary>The milliseconds/percentage delay for a DiscardOnFixedTimeout/DiscardOnPercentageTimeout/Defer throttling strategy</summary>
        public decimal Delay { get; set; }

        /// <summary>Builds a new instance of the Throttling class</summary>
        public Throttling()
        {
            Limit = 0;
            Strategy = ThrottlingStrategy.None;
            Delay = 0;
        }

        internal void EnsureAllowedValues()
        {
            if (Limit < 1)
                Limit = 0;
            if (Delay < 0)
                Delay = 0;
            if (Limit == 0)
                Strategy = ThrottlingStrategy.None;
        }

        internal void Apply(int waitingLogEntries, Action<int> actionWithTimeout)
        {
            if (Strategy == ThrottlingStrategy.None || waitingLogEntries < Limit)
            {
                actionWithTimeout(0);
                return;
            }

            if (Strategy == ThrottlingStrategy.Discard)
            {
                InternalLogger.Warn("Applied discard throttling strategy");
                return;
            }

            ApplyDeferment(waitingLogEntries);
            actionWithTimeout(CalculateTimeout(waitingLogEntries));
        }

        private void ApplyDeferment(int waitingLogEntries)
        {
            var deferStrategy = Strategy == ThrottlingStrategy.DeferForFixedTime ||
                Strategy == ThrottlingStrategy.DeferForPercentageTime;

            if (!deferStrategy)
                return;

            var delay = FixedTime(Delay, waitingLogEntries);
            InternalLogger.Warn($"Applying defer throttling strategy ({delay} ms)");
            Thread.SpinWait(delay);
        }

        private int CalculateTimeout(int waitingLogEntries)
        {
            var timeoutStrategy = Strategy == ThrottlingStrategy.DiscardOnFixedTimeout ||
                Strategy == ThrottlingStrategy.DiscardOnPercentageTimeout;

            if (!timeoutStrategy)
                return Timeout.Infinite;

            var timeout = FixedTime(Delay, waitingLogEntries);
            InternalLogger.Warn($"Applying timeout throttling strategy ({timeout} ms)");
            return timeout;
        }

        private int FixedTime(decimal delay, int waitingLogEntries)
        {
            var isPercentageDelay = Strategy == ThrottlingStrategy.DiscardOnPercentageTimeout ||
                Strategy == ThrottlingStrategy.DeferForPercentageTime;

            if (!isPercentageDelay)
                return (int)delay;

            var fixedTime = waitingLogEntries * delay / 100;
            return (int)fixedTime;
        }
    }
}