using MonJobs.Take;

namespace MonJobs.Tests.Take
{
    /// <summary>
    /// Options related to setting up subscriptions about what to do given jobs are available matching the criteria.
    /// </summary>
    public class TakeNextSubscriptionOptions : SubscriptionOptionsBase
    {
        public TakeNextOptions TakeNextOptions { get; set; }
    }
}