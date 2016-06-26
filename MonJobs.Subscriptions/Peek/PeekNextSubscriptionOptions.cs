using System;
using System.Threading.Tasks;
using MonJobs.Peek;

namespace MonJobs.Subscriptions.Peek
{
    public class PeekNextSubscriptionOptions : SubscriptionOptionsBase
    {
        public PeekNextOptions PeekNextOptions { get; set; }
        public Func<Exception, Task> OnException { get; set; }
    }
}