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

        public Task Subscribe(Func<Job, Task> whatToDo, TakeNextSubscriptionOptions options)
        {
            return Subscribe(async () =>
            {
                var job = await _jobTakeNextService.TakeFor(options.TakeNextOptions).ConfigureAwait(false);
                if (job == null) return false;
                await whatToDo(job).ConfigureAwait(false);
                return true;
            }, options);
        }
    }
}