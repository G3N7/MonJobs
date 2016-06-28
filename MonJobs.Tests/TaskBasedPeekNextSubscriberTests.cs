using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonJobs.Peek;
using MonJobs.Subscriptions.Peek;
using Moq;
using NUnit.Framework;

namespace MonJobs.Tests
{
    internal class TaskBasedPeekNextSubscriberTests
    {
        [Test]
        public async Task Subscribe_WillCheckMoreThanMoreThanOnce()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var exampleOptions = new PeekNextSubscriptionOptions
            {
                Token = cancel.Token,
                PollingInterval = TimeSpan.FromMilliseconds(1),
                PeekNextOptions = new PeekNextOptions()
            };
            var mockPeekNextService = new Mock<IJobPeekNextService>();

            var sut = new TaskBasedPeekNextSubscriber(mockPeekNextService.Object);

            var hasBeenCalled = false;
            await sut.Subscribe(job => Task.FromResult(hasBeenCalled = true), exampleOptions);

            Thread.Sleep(10);

            cancel.Cancel();

            Assert.That(hasBeenCalled, Is.False);
            mockPeekNextService.Verify(x => x.PeekFor(exampleOptions.PeekNextOptions), Times.AtLeast(2));
        }

        [Test]
        public async Task Subscribe_WillNotCallWorkFuncIfNothingMetTheCriteria()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var exampleOptions = new PeekNextSubscriptionOptions
            {
                Token = cancel.Token,
                PollingInterval = TimeSpan.FromMilliseconds(1),
                PeekNextOptions = new PeekNextOptions()
            };
            var mockPeekNextService = new Mock<IJobPeekNextService>();
            mockPeekNextService
                .Setup(x => x.PeekFor(exampleOptions.PeekNextOptions))
                .ReturnsAsync(Enumerable.Empty<Job>());

            var sut = new TaskBasedPeekNextSubscriber(mockPeekNextService.Object);

            var numberOfTimesOurDelegateIsInvoked = 0;
            await sut.Subscribe(job => Task.FromResult(numberOfTimesOurDelegateIsInvoked += 1), exampleOptions);

            Thread.Sleep(10);

            cancel.Cancel();

            Assert.That(numberOfTimesOurDelegateIsInvoked, Is.EqualTo(0));
            mockPeekNextService.Verify(x => x.PeekFor(exampleOptions.PeekNextOptions), Times.AtLeastOnce);
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        public async Task Subscribe_WillFindMessages(int numberOfJobs)
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var exampleOptions = new PeekNextSubscriptionOptions
            {
                Token = cancel.Token,
                PollingInterval = TimeSpan.FromMilliseconds(1),
                PeekNextOptions = new PeekNextOptions()
            };
            var mockPeekNextService = new Mock<IJobPeekNextService>();
            var numberOfJobsTaken = 0;
            mockPeekNextService.Setup(x => x.PeekFor(exampleOptions.PeekNextOptions)).Returns(() =>
            {
                numberOfJobsTaken += 1;
                return numberOfJobsTaken <= numberOfJobs ? Task.FromResult(new[] { new Job() }.AsEnumerable()) : Task.FromResult(Enumerable.Empty<Job>());
            });

            var sut = new TaskBasedPeekNextSubscriber(mockPeekNextService.Object);

            var numberOfTimesOurDelegateIsInvoked = 0;
            await sut.Subscribe(job => Task.FromResult(numberOfTimesOurDelegateIsInvoked += 1), exampleOptions);

            do
            {
                Thread.Sleep(10);
                // wait for us to have processed the jobs, or for the max time to be over.
            } while (numberOfTimesOurDelegateIsInvoked < numberOfJobs && !cancel.IsCancellationRequested);

            cancel.Cancel();

            Assert.That(numberOfTimesOurDelegateIsInvoked, Is.EqualTo(numberOfJobs));
            mockPeekNextService.Verify(x => x.PeekFor(exampleOptions.PeekNextOptions), Times.AtLeast(numberOfJobsTaken));
        }
    }
}
