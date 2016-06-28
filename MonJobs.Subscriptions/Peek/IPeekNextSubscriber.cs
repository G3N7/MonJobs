using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonJobs.Subscriptions.Peek
{
    public interface IPeekNextSubscriber
    {
        // doc: IPeekNextSubscriber
        Task Subscribe(Func<IEnumerable<Job>, Task> whatToDo, PeekNextSubscriptionOptions options);
    }
}