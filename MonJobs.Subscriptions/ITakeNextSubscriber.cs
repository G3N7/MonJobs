using System;
using System.Threading.Tasks;

namespace MonJobs.Subscriptions
{
    public interface ITakeNextSubscriber
    {
        Task Subscribe(QueueId queue, Func<Job, Task> whatToDo, TakeNextSubscriptionOptions options);
    }
}
