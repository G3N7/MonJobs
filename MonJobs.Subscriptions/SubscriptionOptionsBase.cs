using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonJobs.Subscriptions
{
    // doc: SubscriptionOptionsBase
    public abstract class SubscriptionOptionsBase
    {
        public TimeSpan? PollingInterval { get; set; }
        public CancellationToken? Token { get; set; }
        public Func<Exception, Task> OnException { get; set; }
    }
}