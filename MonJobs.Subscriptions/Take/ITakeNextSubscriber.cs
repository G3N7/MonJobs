using System;
using System.Threading.Tasks;

namespace MonJobs.Subscriptions.Take
{
    public interface ITakeNextSubscriber
    {
        // doc: ITakeNextSubscriber
        Task Subscribe(QueueId queue, Func<Job, Task> whatToDo, TakeNextSubscriptionOptions options);
    }
}
