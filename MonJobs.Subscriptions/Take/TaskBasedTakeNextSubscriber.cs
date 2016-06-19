using System;
using System.Threading;
using System.Threading.Tasks;
using MonJobs.Take;

namespace MonJobs.Subscriptions.Take
{
    public class TaskBasedTakeNextSubscriber : ITakeNextSubscriber
    {
        private readonly IJobTakeNextService _jobTakeNextService;

        public TaskBasedTakeNextSubscriber(IJobTakeNextService jobTakeNextService)
        {
            _jobTakeNextService = jobTakeNextService;
        }

        public Task Subscribe(QueueId queue, Func<Job, Task> whatToDo, TakeNextSubscriptionOptions options)
        {
            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    var job = await _jobTakeNextService.TakeFor(options.TakeNextOptions);
                    if (job != null)
                    {
                        await whatToDo(job);
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