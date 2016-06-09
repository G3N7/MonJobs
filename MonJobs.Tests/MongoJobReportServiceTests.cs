using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using NUnit.Framework;

namespace MonJobs.Tests
{
    class MongoJobReportServiceTests : MongoTestBase
    {
        [Test]
        public async Task AddReport_AcknowledgedJob_AddsReport()
        {
            var exampleQueue = QueueId.Parse("ExampleQueue");

            var myJobId = JobId.Generate();
            var existingJobs = new[]{new Job
            {
                Id = myJobId,
                QueueId = exampleQueue,
                Acknowledgment = new JobAcknowledgment
                {
                    {"RunnerId", Guid.NewGuid().ToString("N") }
                }
            }};

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();
                await jobs.InsertManyAsync(existingJobs);


                var sut = new MongoJobReportSerivce(database);
                await sut.AddReport(exampleQueue, myJobId, new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", "FooBar" } });

                var myJob = await jobs.Find(Builders<Job>.Filter.Eq(x => x.Id, myJobId)).FirstAsync();

                Assert.That(myJob.Reports, Has.Length.EqualTo(1));
                Assert.That(myJob.Reports.First().Keys, Contains.Item("Timestamp"));
                Assert.That(myJob.Reports.First().Keys, Contains.Item("Message"));
            });
        }

        [Test]
        public async Task AddReport_AcknowledgedJobWithReports_AddsReport()
        {
            var exampleQueue = QueueId.Parse("ExampleQueue");

            var myJobId = JobId.Generate();
            var existingJobs = new[]{new Job
            {
                Id = myJobId,
                QueueId = exampleQueue,
                Acknowledgment = new JobAcknowledgment
                {
                    {"RunnerId", Guid.NewGuid().ToString("N") }
                },
                Reports = new []{ new JobReport { {"Start Time", DateTime.UtcNow.ToString("O")} } }
            }};

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();
                await jobs.InsertManyAsync(existingJobs);


                var sut = new MongoJobReportSerivce(database);
                await sut.AddReport(exampleQueue, myJobId, new JobReport { { "Timestamp", DateTime.UtcNow.ToString("O") }, { "Message", "FooBar" } });

                var myJob = await jobs.Find(Builders<Job>.Filter.Eq(x => x.Id, myJobId)).FirstAsync();

                Assert.That(myJob.Reports, Has.Length.EqualTo(2));
                Assert.That(myJob.Reports.Last().Keys, Contains.Item("Timestamp"));
                Assert.That(myJob.Reports.Last().Keys, Contains.Item("Message"));
            });
        }
    }
}