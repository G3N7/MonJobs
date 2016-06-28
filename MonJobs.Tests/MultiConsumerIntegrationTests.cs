using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MonJobs.Peek;
using MonJobs.Subscriptions.Peek;
using MonJobs.Subscriptions.Take;
using MonJobs.Take;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MonJobs.Tests
{
    internal class MultiConsumerIntegrationTests : MongoUsecaseIntegrationTestBase
    {
        private static readonly TimeSpan MaxTestTime = TimeSpan.FromSeconds(10);

        [TestCase(10, 2)]
        [TestCase(100, 5)]
        public async Task ProcessJobsForMyDataCenter_XInQueueWithYConsumers_PeekThenAckApproach(int numberOfJobsToQueue, int numberOfConsumers)
        {
            var testStartTime = DateTime.Now;
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

                var finishedJobs = new ConcurrentBag<Job>();

                var cancellationSource = new CancellationTokenSource();
                cancellationSource.CancelAfter(MaxTestTime);
                var cancellationToken = cancellationSource.Token;

                stopwatch.Reset();
                stopwatch.Start();


                for (int i = 0; i < numberOfConsumers; i++)
                {
                    var myDatacenter = new JobAttributes { { "DataCenter", "CAL01" } };

                    await SetupPeekThenAckWorkerWithSubscription(
                            database,
                            exampleQueueName,
                            finishedJobs,
                            myDatacenter,
                            cancellationToken);
                }

                do
                {
                    Thread.Sleep(100);
                } while (finishedJobs.Count < jobsInQueue.Count && DateTime.Now < testStartTime.Add(MaxTestTime));

                stopwatch.Stop();
                Console.WriteLine($"{numberOfConsumers} consumers processed {finishedJobs.Count} jobs in: {stopwatch.ElapsedMilliseconds} ms");

                cancellationSource.Cancel();

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
            var testStartTime = DateTime.Now;
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

                var finishedJobs = new ConcurrentBag<Job>();

                var cancellationSource = new CancellationTokenSource();
                cancellationSource.CancelAfter(MaxTestTime);
                var cancellationToken = cancellationSource.Token;

                stopwatch.Reset();
                stopwatch.Start();

                for (int i = 0; i < numberOfConsumers; i++)
                {

                    var myDataCenter = new JobAttributes { { "DataCenter", "CAL01" } };

                    await SetupTakeNextWorkerWithSubscription(database, exampleQueueName, finishedJobs, myDataCenter, cancellationToken);
                }

                do
                {
                    Thread.Sleep(100);
                } while (finishedJobs.Count < jobsInQueue.Count && DateTime.Now < testStartTime.Add(MaxTestTime));

                stopwatch.Stop();
                Console.WriteLine($"{numberOfConsumers} consumers processed {numberOfJobsToQueue} jobs in: {stopwatch.ElapsedMilliseconds} ms");

                cancellationSource.Cancel();

                var finishedJobIds = finishedJobs.Select(x => x.Id).ToList();
                foreach (var jobId in jobsInQueue)
                {
                    Assert.That(finishedJobIds, Contains.Item(jobId));
                }
            });
        }

        [Test]
        public async Task RollingDeploy_ModelSubJobsOrchestratedByParent()
        {
            var exampleQueueName = QueueId.Parse("Wind");

            await RunInMongoLand(async database =>
            {

                var cancellationSource = new CancellationTokenSource();
                cancellationSource.CancelAfter(MaxTestTime);
                var cancellationToken = cancellationSource.Token;

                var finishedJobs = new ConcurrentBag<Job>();

                await SetupTakeNextWorkerWithSubscription(database, exampleQueueName, finishedJobs, new JobAttributes { { "Name", "DeployServer" }, }, cancellationToken);

                // Create a rolling deploy relevant for my datacenter
                var creationService = new MongoJobCreationService(database);
                await creationService.Create(exampleQueueName, new JobAttributes
                {
                    { "Name", "RollingDeploy" },
                    { "Environment", "Prod" }
                });

                // Take Next available job
                var takeNextSubscriber = new TaskBasedTakeNextSubscriber(new MongoJobTakeNextService(database));
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
                await takeNextSubscriber.Subscribe(async rollingDeployJob =>
                {
                    // Send Reports
                    var reportService = new MongoJobReportService(database);
                    await
                        reportService.AddReport(exampleQueueName, rollingDeployJob.Id,
                            new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", "Starting Rolling Deploy" } });

                    var queryService = new MongoJobQueryService(database);

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
                            hasResult = (await queryService.QueryFor(new JobQuery { QueueId = exampleQueueName, JobIds = new[] { deployServerJobId }, HasResult = true })).FirstOrDefault();
                            Thread.Sleep(500);
                        } while (hasResult == null);

                        await reportService.AddReport(exampleQueueName, rollingDeployJob.Id,
                            new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", $"Deploy on server {server} Completed, {JsonConvert.SerializeObject(hasResult.Result)}" } });
                        // inspect result
                    }

                    // Send Result
                    var completionService = new MongoJobCompletionService(database);
                    await completionService.Complete(exampleQueueName, rollingDeployJob.Id, new JobResult { { "Result", "Success" } });

                    var finalizedRollingDeployJob = await queryService.QueryFor(new JobQuery { QueueId = exampleQueueName, JobIds = new[] { rollingDeployJob.Id } });

                    cancellationSource.Cancel();

                    Console.WriteLine($"Finalized Rolling Deploy Job: {JsonConvert.SerializeObject(finalizedRollingDeployJob)}");

                    Assert.That(finalizedRollingDeployJob.First().Result["Result"], Is.EqualTo("Success"));
                }, new TakeNextSubscriptionOptions
                {
                    TakeNextOptions = peekQuery,
                    Token = cancellationToken,
                });
            });
        }

        private static Task SetupTakeNextWorkerWithSubscription(IMongoDatabase database, QueueId queueName, ConcurrentBag<Job> finishedJobs, JobAttributes attributesThatShouldWork, CancellationToken cancellationToken)
        {
            var standardAck = new JobAcknowledgment
            {
                {"RunnerId", Guid.NewGuid().ToString("N")}
            };

            var options = new TakeNextSubscriptionOptions
            {
                Token = cancellationToken,
                TakeNextOptions = new TakeNextOptions
                {
                    QueueId = queueName,
                    Acknowledgment = standardAck,
                    HasAttributes = attributesThatShouldWork
                }
            };

            var subscriber = new TaskBasedTakeNextSubscriber(new MongoJobTakeNextService(database));
            return subscriber.Subscribe(async nextJob =>
            {
                var exampleReportMessage1 = "FooBar";
                var exampleReportMessage2 = "WizBang";
                var exampleReportMessage3 = "PowPop";

                // Send Reports
                var reportService = new MongoJobReportService(database);
                await
                    reportService.AddReport(queueName, nextJob.Id,
                        new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", exampleReportMessage1 } });
                await
                    reportService.AddReport(queueName, nextJob.Id,
                        new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", exampleReportMessage2 } });
                await
                    reportService.AddReport(queueName, nextJob.Id,
                        new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", exampleReportMessage3 } });

                // Send Result
                var completionService = new MongoJobCompletionService(database);
                await completionService.Complete(queueName, nextJob.Id, new JobResult { { "Result", "Success" } });

                // Finish Job
                var finishedJobFromDb =
                    await database.GetJobCollection()
                    .Find(Builders<Job>.Filter.Eq(x => x.Id, nextJob.Id))
                    .FirstAsync(cancellationToken);

                Assert.That(finishedJobFromDb, Is.Not.Null);
                Assert.That(finishedJobFromDb.Acknowledgment, Is.Not.Null);

                Assert.That(finishedJobFromDb.Reports, Has.Length.EqualTo(3));
                var valuesOfReports = finishedJobFromDb.Reports.SelectMany(x => x.Values).ToList();
                Assert.That(valuesOfReports, Contains.Item(exampleReportMessage1));
                Assert.That(valuesOfReports, Contains.Item(exampleReportMessage2));
                Assert.That(valuesOfReports, Contains.Item(exampleReportMessage3));

                Assert.That(finishedJobFromDb.Result, Is.Not.Null);
                finishedJobs.Add(finishedJobFromDb);
            }, options);
        }

        private static Task SetupPeekThenAckWorkerWithSubscription(IMongoDatabase database, QueueId queueName, ConcurrentBag<Job> finishedJobs, JobAttributes attributesThatShouldWork, CancellationToken cancellationToken)
        {
            var standardAck = new JobAcknowledgment
            {
                {"RunnerId", Guid.NewGuid().ToString("N")}
            };

            var options = new PeekNextSubscriptionOptions()
            {
                Token = cancellationToken,
                PeekNextOptions = new PeekNextOptions
                {
                    QueueId = queueName,
                    HasAttributes = attributesThatShouldWork
                }
            };

            var subscriber = new TaskBasedPeekNextSubscriber(new MongoJobPeekNextService(new MongoJobQueryService(database)));
            return subscriber.Subscribe(async nextJobs =>
            {
                var nextJob = nextJobs.First();
                // Acknowledge the job
                var acknowledgmentService = new MongoJobAcknowledgmentService(database);

                var ackResult = await acknowledgmentService.Ack(queueName, nextJob.Id, standardAck);
                if (!ackResult.Success) return;

                var exampleReportMessage1 = "FooBar";
                var exampleReportMessage2 = "WizBang";
                var exampleReportMessage3 = "PowPop";

                // Send Reports
                var reportService = new MongoJobReportService(database);
                await
                    reportService.AddReport(queueName, nextJob.Id,
                        new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", exampleReportMessage1 } });
                await
                    reportService.AddReport(queueName, nextJob.Id,
                        new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", exampleReportMessage2 } });
                await
                    reportService.AddReport(queueName, nextJob.Id,
                        new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", exampleReportMessage3 } });

                // Send Result
                var completionService = new MongoJobCompletionService(database);
                await completionService.Complete(queueName, nextJob.Id, new JobResult { { "Result", "Success" } });

                // Finish Job
                var finishedJobFromDb =
                    await database.GetJobCollection().Find(Builders<Job>.Filter.Eq(x => x.Id, nextJob.Id)).FirstAsync(cancellationToken);

                Assert.That(finishedJobFromDb, Is.Not.Null);
                Assert.That(finishedJobFromDb.Acknowledgment, Is.Not.Null);

                Assert.That(finishedJobFromDb.Reports, Has.Length.EqualTo(3));
                var valuesOfReports = finishedJobFromDb.Reports.SelectMany(x => x.Values).ToList();
                Assert.That(valuesOfReports, Contains.Item(exampleReportMessage1));
                Assert.That(valuesOfReports, Contains.Item(exampleReportMessage2));
                Assert.That(valuesOfReports, Contains.Item(exampleReportMessage3));

                Assert.That(finishedJobFromDb.Result, Is.Not.Null);
                finishedJobs.Add(finishedJobFromDb);
            }, options);
        }
    }
}