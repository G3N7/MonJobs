using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MonJobs.Take;
using NUnit.Framework;

namespace MonJobs.Tests
{
    internal class MongoJobTakeNextServiceTests : MongoTestBase
    {
        [Test]
        public async Task TakeFor_NewJobs_ReturnsAvailableJobAndAcknowledges()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleOptions = new TakeNextOptions
            {
                QueueId = exampleQueueId,
                Acknowledgment = new JobAcknowledgment
                {
                    {"RunnerId", Guid.NewGuid().ToString("N") }
                }
            };

            var newlyCreatedJobId = JobId.Generate();
            var existingJobs = new[] { new Job
            {
                Id = JobId.Generate(),
                QueueId = exampleQueueId,
                Result = new JobResult {
                    { "Result", "Success"}
                },
                Acknowledgment = new JobAcknowledgment
                {
                    { "RunnerId", Guid.NewGuid().ToString("N") }
                }
            }, new Job
            {
                Id = JobId.Generate(),
                QueueId = exampleQueueId,
                Acknowledgment = new JobAcknowledgment
                {
                    { "RunnerId", Guid.NewGuid().ToString("N") }
                }
            }, new Job
            {
                Id = newlyCreatedJobId,
                QueueId = exampleQueueId
            } };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobTakeNextService(database);

                var result = await sut.TakeFor(exampleOptions).ConfigureAwait(false);

                Assert.That(result, Is.Not.Null);
                Assert.That(result?.Id, Is.EqualTo(newlyCreatedJobId));

                var jobInDb = await jobs.Find(Builders<Job>.Filter.Eq(x => x.Id, result?.Id)).SingleAsync().ConfigureAwait(false);
                Assert.That(jobInDb.Acknowledgment, Is.Not.Null);
                Assert.That(jobInDb.Acknowledgment["RunnerId"], Is.EqualTo(exampleOptions.Acknowledgment["RunnerId"]));
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task TakeFor_NewJobs_OnlyModifiesOneJob()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleOptions = new TakeNextOptions
            {
                QueueId = exampleQueueId,
                Acknowledgment = new JobAcknowledgment
                {
                    {"RunnerId", Guid.NewGuid().ToString("N") }
                }
            };

            var newlyCreatedJobId1 = JobId.Generate();
            var newlyCreatedJobId2 = JobId.Generate();

            var existingJobs = new[] { new Job
            {
                Id = JobId.Generate(),
                QueueId = exampleQueueId,
                Result = new JobResult {
                    { "Result", "Success"}
                },
                Acknowledgment = new JobAcknowledgment
                {
                    { "RunnerId", Guid.NewGuid().ToString("N") }
                }
            }, new Job
            {
                Id = JobId.Generate(),
                QueueId = exampleQueueId,
                Acknowledgment = new JobAcknowledgment
                {
                    { "RunnerId", Guid.NewGuid().ToString("N") }
                }
            }, new Job
            {
                Id = newlyCreatedJobId1,
                QueueId = exampleQueueId
            }, new Job
            {
                Id = newlyCreatedJobId2,
                QueueId = exampleQueueId
            } };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobTakeNextService(database);

                var result = await sut.TakeFor(exampleOptions).ConfigureAwait(false);

                Assert.That(result, Is.Not.Null);
                Assert.That(result?.Id, Is.EqualTo(newlyCreatedJobId1));

                var jobInDb1 = await jobs.Find(Builders<Job>.Filter.Eq(x => x.Id, newlyCreatedJobId1)).SingleAsync().ConfigureAwait(false);
                Assert.That(jobInDb1.Acknowledgment, Is.Not.Null);
                Assert.That(jobInDb1.Acknowledgment["RunnerId"], Is.EqualTo(exampleOptions.Acknowledgment["RunnerId"]));

                var jobInDb2 = await jobs.Find(Builders<Job>.Filter.Eq(x => x.Id, newlyCreatedJobId2)).SingleAsync().ConfigureAwait(false);
                Assert.That(jobInDb2.Acknowledgment, Is.Null);
            }).ConfigureAwait(false);
        }
    }
}