using System;
using System.Threading.Tasks;
using MonJobs.Take;

namespace MonJobs.Subscriptions.Take
{
    // doc: TakeNextSubscriptionOptions
    public class TakeNextSubscriptionOptions : SubscriptionOptionsBase
    {
        public TakeNextOptions TakeNextOptions { get; set; }
    }
}