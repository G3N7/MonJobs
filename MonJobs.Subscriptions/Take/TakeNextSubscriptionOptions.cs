using System;
using System.Threading.Tasks;
using MonJobs.Take;

namespace MonJobs.Subscriptions.Take
{
    public class TakeNextSubscriptionOptions : SubscriptionOptionsBase
    {
        public TakeNextOptions TakeNextOptions { get; set; }

        public Func<Exception, Task> OnException { get; set; }
    }
}