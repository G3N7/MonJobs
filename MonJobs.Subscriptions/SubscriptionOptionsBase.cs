using System;
using System.Threading;

namespace MonJobs.Subscriptions
{
    public abstract class SubscriptionOptionsBase
    {
        public TimeSpan? PollingInterval { get; set; }
        public CancellationToken? Token { get; set; }
    }
}