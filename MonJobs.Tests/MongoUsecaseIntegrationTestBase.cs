using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MonJobs.Peek;
using MonJobs.Take;
using NUnit.Framework;

namespace MonJobs.Tests
{
    internal class MongoUsecaseIntegrationTestBase : MongoTestBase
    {
        protected static async Task<Job> TryProcessOneJobUsingPeekThanAck(IMongoDatabase database, QueueId exampleQueueName, JobAttributes attributesThatShouldWork)
        {
            // Query for next available a job for my datacenter
            var peekNextService = new MongoJobPeekNextService(new MongoJobQueryService(database));
            var peekQuery = new PeekNextOptions
            {
                QueueId = exampleQueueName,
                HasAttributes = attributesThatShouldWork,
                Limit = 5
            };
            var jobs = (await peekNextService.PeekFor(peekQuery)).ToList();
            if (!jobs.Any()) return null;

            foreach (var job in jobs)
            {
                // Acknowledge the job
                var acknowledgmentService = new MongoJobAcknowledgmentService(database);
                var standardAck = new JobAcknowledgment
                {
                    {"RunnerId", Guid.NewGuid().ToString("N")}
                };
                var ackResult = await acknowledgmentService.Ack(exampleQueueName, job.Id, standardAck);
                if (!ackResult.Success) continue;

                var exampleReportMessage1 = "FooBar";
                var exampleReportMessage2 = "WizBang";
                var exampleReportMessage3 = "PowPop";

                // Send Reports
                var reportService = new MongoJobReportService(database);
                await
                    reportService.AddReport(exampleQueueName, job.Id,
                        new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", exampleReportMessage1 } });
                await
                    reportService.AddReport(exampleQueueName, job.Id,
                        new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", exampleReportMessage2 } });
                await
                    reportService.AddReport(exampleQueueName, job.Id,
                        new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", exampleReportMessage3 } });

                // Send Result
                var completionService = new MongoJobCompletionService(database);
                await completionService.Complete(exampleQueueName, job.Id, new JobResult { { "Result", "Success" } });

                // Finish Job
                var finishedJobFromDb =
                    await database.GetJobCollection().Find(Builders<Job>.Filter.Eq(x => x.Id, job.Id)).FirstAsync();

                Assert.That(finishedJobFromDb, Is.Not.Null);
                Assert.That(finishedJobFromDb.Acknowledgment, Is.Not.Null);

                Assert.That(finishedJobFromDb.Reports, Has.Length.EqualTo(3));
                var valuesOfReports = finishedJobFromDb.Reports.SelectMany(x => x.Values).ToList();
                Assert.That(valuesOfReports, Contains.Item(exampleReportMessage1));
                Assert.That(valuesOfReports, Contains.Item(exampleReportMessage2));
                Assert.That(valuesOfReports, Contains.Item(exampleReportMessage3));

                Assert.That(finishedJobFromDb.Result, Is.Not.Null);

                return finishedJobFromDb;
            }
            return null;
        }

        protected static async Task<Job> TryProcessOneJobUsingTakeNext(IMongoDatabase database, QueueId exampleQueueName, JobAttributes attributesThatShouldWork)
        {
            // Take Next available job
            var takeNextService = new MongoJobTakeNextService(database);
            var standardAck = new JobAcknowledgment
            {
                {"RunnerId", Guid.NewGuid().ToString("N")}
            };
            var peekQuery = new TakeNextOptions
            {
                QueueId = exampleQueueName,
                HasAttributes = attributesThatShouldWork,
                Acknowledgment = standardAck
            };
            var nextJob = await takeNextService.TakeFor(peekQuery);
            if (nextJob == null) return null;

            var exampleReportMessage1 = "FooBar";
            var exampleReportMessage2 = "WizBang";
            var exampleReportMessage3 = "PowPop";

            // Send Reports
            var reportService = new MongoJobReportService(database);
            await
                reportService.AddReport(exampleQueueName, nextJob.Id,
                    new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", exampleReportMessage1 } });
            await
                reportService.AddReport(exampleQueueName, nextJob.Id,
                    new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", exampleReportMessage2 } });
            await
                reportService.AddReport(exampleQueueName, nextJob.Id,
                    new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", exampleReportMessage3 } });

            // Send Result
            var completionService = new MongoJobCompletionService(database);
            await completionService.Complete(exampleQueueName, nextJob.Id, new JobResult { { "Result", "Success" } });

            // Finish Job
            var finishedJobFromDb =
                await database.GetJobCollection().Find(Builders<Job>.Filter.Eq(x => x.Id, nextJob.Id)).FirstAsync();

            Assert.That(finishedJobFromDb, Is.Not.Null);
            Assert.That(finishedJobFromDb.Acknowledgment, Is.Not.Null);

            Assert.That(finishedJobFromDb.Reports, Has.Length.EqualTo(3));
            var valuesOfReports = finishedJobFromDb.Reports.SelectMany(x => x.Values).ToList();
            Assert.That(valuesOfReports, Contains.Item(exampleReportMessage1));
            Assert.That(valuesOfReports, Contains.Item(exampleReportMessage2));
            Assert.That(valuesOfReports, Contains.Item(exampleReportMessage3));

            Assert.That(finishedJobFromDb.Result, Is.Not.Null);

            return finishedJobFromDb;
        }
    }
}