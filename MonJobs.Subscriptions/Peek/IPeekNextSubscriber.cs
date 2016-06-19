using System.Threading.Tasks;

namespace MonJobs.Subscriptions.Peek
{
    public interface IPeekNextSubscriber
    {
        Task Subscribe(QueueId queue, PeekNextSubscriptionOptions options);
    }
}