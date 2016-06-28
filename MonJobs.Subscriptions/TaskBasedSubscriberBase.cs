using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonJobs.Subscriptions
{
    public abstract class TaskBasedSubscriberBase
    {
        protected static Task Subscribe(Func<Task<bool>> whatToDoReturningIfNoWorkWasNeeded, SubscriptionOptionsBase options)
        {
            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    var hadSomethingToDo = await whatToDoReturningIfNoWorkWasNeeded();
                    if (!hadSomethingToDo)
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
                Subscribe(whatToDoReturningIfNoWorkWasNeeded, options);
#pragma warning restore 4014
                // if the consumer has configured a cancellation token, use it.
            }, options?.Token ?? CancellationToken.None);
        }
    }
}