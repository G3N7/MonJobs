using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonJobs.Subscriptions.Peek
{
    public interface IPeekNextSubscriber
    {
        Task Subscribe(QueueId queue, Func<IEnumerable<Job>, Task> whatToDo, PeekNextSubscriptionOptions options);
    }
}