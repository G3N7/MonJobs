using System;
using System.Threading.Tasks;
using MonJobs.Take;

namespace MonJobs.Subscriptions
{
    public class TakeNextSubscriptionOptions : SubscriptionOptionsBase
    {
        public TakeNextOptions TakeNextOptions { get; set; }

        public Func<Exception, Task> OnException { get; set; }
    }
}