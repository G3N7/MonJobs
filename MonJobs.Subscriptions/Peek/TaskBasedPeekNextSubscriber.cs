using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonJobs.Peek;

namespace MonJobs.Subscriptions.Peek
{


    public class TaskBasedPeekNextSubscriber : IPeekNextSubscriber
    {
        private readonly IJobPeekNextService _jobPeekNextService;

        public TaskBasedPeekNextSubscriber(IJobPeekNextService jobPeekNextService)
        {
            _jobPeekNextService = jobPeekNextService;
        }

        public Task Subscribe(QueueId queue, Func<IEnumerable<Job>, Task> whatToDo, PeekNextSubscriptionOptions options)
        {
            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    var jobs = await _jobPeekNextService.PeekFor(options.PeekNextOptions);
                    var jobsArray = jobs as Job[] ?? jobs.ToArray();
                    if (jobs != null && jobsArray.Any())
                    {
                        await whatToDo(jobsArray);
                    }
                    else
                    {
                        Thread.Sleep(options.PollingInterval ?? TimeSpan.FromSeconds(.5));
                    }
                }
                catch (Exception ex)
                {
                    // if the consumer configured an exception handling mechanism use it.
                    if (options.OnException != null) await options.OnException(ex);
                }

                // Here we are ensuring recursion without having to await.
#pragma warning disable 4014
                Subscribe(queue, whatToDo, options);
#pragma warning restore 4014
                // if the consumer has configured a cancellation token, use it.
            }, options?.Token ?? CancellationToken.None);
        }
    }
}