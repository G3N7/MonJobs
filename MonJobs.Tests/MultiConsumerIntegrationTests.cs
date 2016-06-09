using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using NUnit.Framework;

namespace MonJobs.Tests
{
    public class MultiConsumerIntegrationTests : MongoUsecaseIntegrationTestBase
    {
        [TestCase(10, 2)]
        [TestCase(100, 5)]
        [TestCase(1000, 15)]
        public async Task ProcessJobsForMyDataCenter_XInQueueWithYConsumers_PeekThenAckApproach(int numberOfJobsToQueue, int numberOfConsumers)
        {
            var exampleQueueName = QueueId.Parse("ExampleQueue");

            await RunInMongoLand(async database =>
            {
                // Create two jobs that are relevant for my datacenter
                var creationService = new MongoJobCreationService(database);
                var jobsInQueue = new List<JobId>();

                var stopwatch = new Stopwatch();
                stopwatch.Start();


                for (int i = 0; i < numberOfJobsToQueue; i++)
                {
                    var newJobId = await creationService.Create(exampleQueueName, new JobAttributes
                    {
                        { "Name", "SpinWidget" },
                        { "DataCenter", "CAL01" },
                    });

                    jobsInQueue.Add(newJobId);
                }

                stopwatch.Stop();
                Console.WriteLine($"Creating {numberOfJobsToQueue} jobs took: {stopwatch.ElapsedMilliseconds} ms");

                var finishedJobs = new List<Job>();

                var tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;

                stopwatch.Reset();
                stopwatch.Start();

                var consumerThreads = new List<Task>();
                for (int i = 0; i < numberOfConsumers; i++)
                {
                    try
                    {
                        consumerThreads.Add(ContinuouslyTryProcessOneJobUsingPeekThanAck(database, exampleQueueName,
                            finishedJobs, token));
                    }
                    catch (AssertionException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                
                do
                {
                    Thread.Sleep(100);
                } while (finishedJobs.Count < jobsInQueue.Count);

                stopwatch.Stop();
                Console.WriteLine($"{numberOfConsumers} consumers processed {numberOfJobsToQueue} jobs in: {stopwatch.ElapsedMilliseconds} ms");

                tokenSource.Cancel();

                var finishedJobIds = finishedJobs.Select(x => x.Id).ToList();
                foreach (var jobId in jobsInQueue)
                {
                    Assert.That(finishedJobIds, Contains.Item(jobId));
                }
            });
        }

        [TestCase(10, 2)]
        [TestCase(100, 5)]
        [TestCase(1000, 15)]
        public async Task ProcessJobsForMyDataCenter_XInQueueWithYConsumers_TakeNextApproach(int numberOfJobsToQueue, int numberOfConsumers)
        {
            var exampleQueueName = QueueId.Parse("ExampleQueue");

            await RunInMongoLand(async database =>
            {
                // Create two jobs that are relevant for my datacenter
                var creationService = new MongoJobCreationService(database);
                var jobsInQueue = new List<JobId>();
                
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                for (int i = 0; i < numberOfJobsToQueue; i++)
                {
                    var newJobId = await creationService.Create(exampleQueueName, new JobAttributes
                    {
                        { "Name", "SpinWidget" },
                        { "DataCenter", "CAL01" },
                    });

                    jobsInQueue.Add(newJobId);
                }

                stopwatch.Stop();
                Console.WriteLine($"Creating {numberOfJobsToQueue} jobs took: {stopwatch.ElapsedMilliseconds} ms");

                var finishedJobs = new List<Job>();

                var tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;

                stopwatch.Reset();
                stopwatch.Start();

                var consumerThreads = new List<Task>();
                for (int i = 0; i < numberOfConsumers; i++)
                {
                    try
                    {
                        consumerThreads.Add(ContinuouslyTryProcessOneJobUsingTakeNext(database, exampleQueueName,
                            finishedJobs, token));
                    }
                    catch (AssertionException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                do
                {
                    Thread.Sleep(100);
                } while (finishedJobs.Count < jobsInQueue.Count);

                stopwatch.Stop();
                Console.WriteLine($"{numberOfConsumers} consumers processed {numberOfJobsToQueue} jobs in: {stopwatch.ElapsedMilliseconds} ms");

                tokenSource.Cancel();

                var finishedJobIds = finishedJobs.Select(x => x.Id).ToList();
                foreach (var jobId in jobsInQueue)
                {
                    Assert.That(finishedJobIds, Contains.Item(jobId));
                }
            });
        }

        private static Task ContinuouslyTryProcessOneJobUsingPeekThanAck(IMongoDatabase database, QueueId queueName, List<Job> finishedJobs, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                var result = TryProcessOneJobUsingPeekThanAck(database, queueName).GetAwaiter().GetResult();

                if (result != null) finishedJobs.Add(result);

                return ContinuouslyTryProcessOneJobUsingPeekThanAck(database, queueName, finishedJobs, cancellationToken);
            }, cancellationToken);
        }

        private static Task ContinuouslyTryProcessOneJobUsingTakeNext(IMongoDatabase database, QueueId queueName, List<Job> finishedJobs, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                var result = TryProcessOneJobUsingPeekThanAck(database, queueName).GetAwaiter().GetResult();

                if (result != null) finishedJobs.Add(result);

                return ContinuouslyTryProcessOneJobUsingTakeNext(database, queueName, finishedJobs, cancellationToken);
            }, cancellationToken);
        }
    }
}