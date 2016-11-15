using MonJobs.Peek;

namespace MonJobs.Tests.Peek
{
    /// <summary>
    /// Options related to setting up subscriptions about what to do given jobs are available matching the criteria.
    /// </summary>
    public class PeekNextSubscriptionOptions : SubscriptionOptionsBase
    {
        public PeekNextOptions PeekNextOptions { get; set; }
    }
}