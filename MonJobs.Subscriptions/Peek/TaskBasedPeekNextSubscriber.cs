using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonJobs.Peek;

namespace MonJobs.Subscriptions.Peek
{


    public class TaskBasedPeekNextSubscriber : TaskBasedSubscriberBase, IPeekNextSubscriber
    {
        private readonly IJobPeekNextService _jobPeekNextService;

        public TaskBasedPeekNextSubscriber(IJobPeekNextService jobPeekNextService)
        {
            _jobPeekNextService = jobPeekNextService;
        }

        public Task Subscribe(Func<IEnumerable<Job>, Task> whatToDo, PeekNextSubscriptionOptions options)
        {
            return Subscribe(async () =>
            {
                var jobs = await _jobPeekNextService.PeekFor(options.PeekNextOptions);
                var jobsArray = jobs as Job[] ?? jobs.ToArray();
                if (jobs == null || !jobsArray.Any()) return false;
                await whatToDo(jobsArray);
                return true;
            }, options);
        }
    }
}