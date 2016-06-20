﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MonJobs.Subscriptions.Take;
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
                PollingInterval = TimeSpan.FromMilliseconds(1),
                TakeNextOptions = new TakeNextOptions()
            };
            var mockPeekNextService = new Mock<IJobTakeNextService>();

            var sut = new TaskBasedTakeNextSubscriber(mockPeekNextService.Object);

            var hasBeenCalled = false;
            await sut.Subscribe(exampleQueueId, job => Task.FromResult(hasBeenCalled = true), exampleOptions);

            Assert.That(hasBeenCalled, Is.False);
            mockPeekNextService.Verify(x => x.TakeFor(exampleOptions.TakeNextOptions), Times.AtLeast(2));
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        public async Task Subscribe_WillFindMessages(int numberOfJobs)
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var exampleOptions = new TakeNextSubscriptionOptions
            {
                Token = cancel.Token,
                PollingInterval = TimeSpan.FromMilliseconds(1),
                TakeNextOptions = new TakeNextOptions()
            };
            var mockPeekNextService = new Mock<IJobTakeNextService>();
            var numberOfJobsTaken = 0;
            mockPeekNextService.Setup(x => x.TakeFor(exampleOptions.TakeNextOptions)).Returns(() =>
            {
                numberOfJobsTaken += 1;
                return numberOfJobsTaken <= numberOfJobs ? Task.FromResult(new Job()) : Task.FromResult(default(Job));
            });

            var sut = new TaskBasedTakeNextSubscriber(mockPeekNextService.Object);

            var numberOfTimesOurDelegateIsInvoked = 0;
            await sut.Subscribe(exampleQueueId, job => Task.FromResult(numberOfTimesOurDelegateIsInvoked += 1), exampleOptions);

            do
            {
                Thread.Sleep(10);
                // wait for us to have processed the jobs, or for the max time to be over.
            } while (numberOfTimesOurDelegateIsInvoked < numberOfJobs && !cancel.IsCancellationRequested);

            cancel.Cancel();

            Assert.That(numberOfTimesOurDelegateIsInvoked, Is.EqualTo(numberOfJobs));
            mockPeekNextService.Verify(x => x.TakeFor(exampleOptions.TakeNextOptions), Times.AtLeast(numberOfJobsTaken));
        }
    }
}