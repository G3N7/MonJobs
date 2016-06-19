using System.Threading.Tasks;

namespace MonJobs.Subscriptions
{
    public interface IPeekNextSubscriber
    {
        Task Subscribe(QueueId queue, PeekNextSubscriptionOptions options);
    }
}