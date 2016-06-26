using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MonJobs.Take;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MonJobs.Tests
{
    public class MultiConsumerIntegrationTests : MongoUsecaseIntegrationTestBase
    {
        [TestCase(10, 2)]
        [TestCase(100, 5)]
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

                        var myDatacenter = new JobAttributes { { "DataCenter", "CAL01" } };
                        consumerThreads.Add(ContinuouslyTryProcessOneJobUsingPeekThanAck(database, exampleQueueName,
                            finishedJobs, myDatacenter, token));
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
                        var myDataCenter = new JobAttributes { { "DataCenter", "CAL01" } };

                        consumerThreads.Add(ContinuouslyTryProcessOneJobUsingTakeNext(database, exampleQueueName,
                            finishedJobs, myDataCenter, token));
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
        
        [Test]
        public async Task RollingDeploy_ModelSubJobsOrchistratedByParent()
        {
            var exampleQueueName = QueueId.Parse("Wind");

            await RunInMongoLand(async database =>
            {

                var cancellationSource = new CancellationTokenSource();

                var cancellationToken = cancellationSource.Token;

                var finishedJobs = new List<Job>();

#pragma warning disable 4014
                ContinuouslyTryProcessOneJobUsingTakeNext(database, exampleQueueName, finishedJobs, new JobAttributes { { "Name", "DeployServer" }, }, cancellationToken);
#pragma warning restore 4014

                // Create a rolling deploy relevant for my datacenter
                var creationService = new MongoJobCreationService(database);
                var rollingDeploy = await creationService.Create(exampleQueueName, new JobAttributes
                {
                    { "Name", "RollingDeploy" },
                    { "Environment", "Prod" }
                });

                // Take Next available job
                var takeNextService = new MongoJobTakeNextService(database);
                var standardAck = new JobAcknowledgment
                {
                    {"RunnerId", Guid.NewGuid().ToString("N")}
                };
                var peekQuery = new TakeNextOptions
                {
                    QueueId = exampleQueueName,
                    HasAttributes = new JobAttributes { { "Environment", "Prod" } },
                    Acknowledgment = standardAck
                };
                var rollingDeployJob = await takeNextService.TakeFor(peekQuery);
                if (rollingDeployJob == null) Assert.Fail();

                // Send Reports
                var reportService = new MongoJobReportService(database);
                await
                    reportService.AddReport(exampleQueueName, rollingDeployJob.Id,
                        new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", "Starting Rolling Deploy" } });

                var queryService = new MongoJobQuerySerivce(database);

                IEnumerable servers = new[] { "PROD2", "PROD1" };
                foreach (var server in servers)
                {
                    var deployServer = new JobAttributes
                    {
                        { "Name", "DeployServer" },
                        { "Server", server }
                    };

                    await reportService.AddReport(exampleQueueName, rollingDeployJob.Id,
                        new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", $"Requesting Deploy on server {server}" } });
                    var deployServerJobId = await creationService.Create(exampleQueueName, deployServer);

                    Job hasResult;
                    do
                    {
                        // replace with detail service
                        hasResult = (await queryService.QueryFor(new JobQuery { QueueId = exampleQueueName, JobIds = new[] { deployServerJobId }, HasResult = true})).FirstOrDefault();
                        Thread.Sleep(500);
                    } while (hasResult == null);

                    await reportService.AddReport(exampleQueueName, rollingDeployJob.Id,
                        new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", $"Deploy on server {server} Completed, {JsonConvert.SerializeObject(hasResult.Result)}" } });
                    // inspect result
                }

                // Send Result
                var completionSerivce = new MongoJobCompletionService(database);
                await completionSerivce.Complete(exampleQueueName, rollingDeployJob.Id, new JobResult { { "Result", "Success" } });

                var finalizedRollingDeployJob = await queryService.QueryFor(new JobQuery { QueueId = exampleQueueName, JobIds = new[] { rollingDeployJob.Id } });

                cancellationSource.Cancel();

                Console.WriteLine($"Finalized Rolling Deploy Job: {JsonConvert.SerializeObject(finalizedRollingDeployJob)}");

                Assert.That(finalizedRollingDeployJob.First().Result["Result"], Is.EqualTo("Success"));
            });
        }


        private static Task ContinuouslyTryProcessOneJobUsingPeekThanAck(IMongoDatabase database, QueueId queueName, List<Job> finishedJobs, JobAttributes attributesThatShouldWork, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                var result = TryProcessOneJobUsingPeekThanAck(database, queueName, attributesThatShouldWork).GetAwaiter().GetResult();

                if (result != null) finishedJobs.Add(result);

                return ContinuouslyTryProcessOneJobUsingPeekThanAck(database, queueName, finishedJobs, attributesThatShouldWork, cancellationToken);
            }, cancellationToken);
        }

        private static Task ContinuouslyTryProcessOneJobUsingTakeNext(IMongoDatabase database, QueueId queueName, List<Job> finishedJobs, JobAttributes attributesThatShouldWork, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                var result = TryProcessOneJobUsingPeekThanAck(database, queueName, attributesThatShouldWork).GetAwaiter().GetResult();

                if (result != null) finishedJobs.Add(result);

                return ContinuouslyTryProcessOneJobUsingTakeNext(database, queueName, finishedJobs, attributesThatShouldWork, cancellationToken);
            }, cancellationToken);
        }
    }
}