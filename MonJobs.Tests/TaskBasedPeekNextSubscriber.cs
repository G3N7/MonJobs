using System;
using System.Threading;
using System.Threading.Tasks;
using MonJobs.Subscriptions;
using MonJobs.Take;
using Moq;
using NUnit.Framework;

namespace MonJobs.Tests
{
    public class TaskBasedPeekNextSubscriberTests
    {
        [Test]
        public async Task Subscribe_WillCheckMoreThanMoreThanOnce()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var exampleOptions = new TakeNextSubscriptionOptions
            {
                Token = cancel.Token,
                PollingInterval = TimeSpan.FromSeconds(1),
                TakeNextOptions = new TakeNextOptions()
            };
            var mockPeekNextService = new Mock<IJobTakeNextService>();

            var sut = new TaskBasedTakeNextSubscriber(mockPeekNextService.Object);

            var hasBeenCalled = false;
            await sut.Subscribe(exampleQueueId, job => Task.FromResult(hasBeenCalled = true), exampleOptions);

            Assert.That(hasBeenCalled, Is.False);
            mockPeekNextService.Verify(x => x.TakeFor(exampleOptions.TakeNextOptions), Times.AtLeast(2));
        }
    }
}
