using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using NUnit.Framework;

namespace MonJobs.Tests
{
    internal class MongoJobReportServiceTests : MongoTestBase
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

            var exampleReport = new JobReport
            {
                {"Timestamp", DateTime.UtcNow.ToString("O")},
                { "Message", "FooBar"}
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();
                await jobs.InsertManyAsync(existingJobs);
                
                var sut = new MongoJobReportService(database);
                await sut.AddReport(exampleQueue, myJobId, exampleReport);

                var myJob = await jobs.Find(Builders<Job>.Filter.Eq(x => x.Id, myJobId)).FirstAsync();

                Assert.That(myJob.Reports, Has.Length.EqualTo(1));
                Assert.That(myJob.Reports.First().Keys, Contains.Item("Timestamp"));
                Assert.That(myJob.Reports.Last()["Timestamp"], Is.EqualTo(exampleReport["Timestamp"]));
                Assert.That(myJob.Reports.First().Keys, Contains.Item("Message"));
                Assert.That(myJob.Reports.Last()["Message"], Is.EqualTo(exampleReport["Message"]));
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

            var exampleReport = new JobReport {{"Timestamp", DateTime.UtcNow.ToString("O")}, {"Message", "FooBar"}};

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();
                await jobs.InsertManyAsync(existingJobs);

                var sut = new MongoJobReportService(database);
                await sut.AddReport(exampleQueue, myJobId, exampleReport);

                var myJob = await jobs.Find(Builders<Job>.Filter.Eq(x => x.Id, myJobId)).FirstAsync();

                Assert.That(myJob.Reports, Has.Length.EqualTo(2));
                Assert.That(myJob.Reports.Last().Keys, Contains.Item("Timestamp"));
                Assert.That(myJob.Reports.Last()["Timestamp"], Is.EqualTo(exampleReport["Timestamp"]));
                Assert.That(myJob.Reports.Last().Keys, Contains.Item("Message"));
                Assert.That(myJob.Reports.Last()["Message"], Is.EqualTo(exampleReport["Message"]));
            });
        }
    }
}