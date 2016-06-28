using System;
using System.Threading.Tasks;
using MonJobs.Take;

namespace MonJobs.Subscriptions.Take
{
    public class TaskBasedTakeNextSubscriber : TaskBasedSubscriberBase, ITakeNextSubscriber
    {
        private readonly IJobTakeNextService _jobTakeNextService;

        public TaskBasedTakeNextSubscriber(IJobTakeNextService jobTakeNextService)
        {
            _jobTakeNextService = jobTakeNextService;
        }

        public Task Subscribe(QueueId queue, Func<Job, Task> whatToDo, TakeNextSubscriptionOptions options)
        {
            return Subscribe(async () =>
            {
                var job = await _jobTakeNextService.TakeFor(options.TakeNextOptions);
                if (job == null) return false;
                await whatToDo(job);
                return true;
            }, options);
        }
    }
}