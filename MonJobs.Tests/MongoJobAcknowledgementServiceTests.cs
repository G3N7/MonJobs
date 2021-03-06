using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using NUnit.Framework;

namespace MonJobs.Tests
{
    internal class MongoJobAcknowledgmentServiceTests : MongoTestBase
    {
        [Test]
        public async Task Ack_ValidInformation_StampsAcknowledgmentReturnsPositiveResult()
        {
            var exampleQueueName = QueueId.Parse("ExampleQueue");

            var readyJobId = JobId.Generate();

            var existingJobs = new[]
            {
                new Job
                {
                    Id = readyJobId,
                    QueueId = exampleQueueName
                }
            };

            var exampleAck = new JobAcknowledgment
                {
                    {"RunnerId", Guid.NewGuid().ToString("N") }
                };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();
                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobAcknowledgmentService(database);
                var result = await sut.Ack(exampleQueueName, readyJobId, exampleAck).ConfigureAwait(false);

                var jobWeExpectToHaveBeenAcknowledged = await jobs.Find(Builders<Job>.Filter.Eq(x => x.Id, readyJobId)).FirstAsync().ConfigureAwait(false);

                Assert.That(result?.Success, Is.True);
                Assert.That(jobWeExpectToHaveBeenAcknowledged.Acknowledgment, Is.EqualTo(exampleAck));
                Assert.That(jobWeExpectToHaveBeenAcknowledged.Acknowledgment["RunnerId"], Is.EqualTo(exampleAck["RunnerId"]));
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task Ack_AlreadyAcknowledged_ReturnsNegativeResult()
        {
            var exampleQueueName = QueueId.Parse("ExampleQueue");

            var readyJobId = JobId.Generate();

            var existingJobs = new[]
            {
                new Job
                {
                    Id = readyJobId,
                    QueueId = exampleQueueName,
                    Acknowledgment = new JobAcknowledgment
                    {
                        {"RunnerId", Guid.NewGuid().ToString("N") }
                    }
                }
            };

            var exampleAck = new JobAcknowledgment
                {
                    {"RunnerId", Guid.NewGuid().ToString("N") }
                };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();
                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobAcknowledgmentService(database);
                var result = await sut.Ack(exampleQueueName, readyJobId, exampleAck).ConfigureAwait(false);

                var jobWeExpectToHaveBeenAcknowledged = await jobs.Find(Builders<Job>.Filter.Eq(x => x.Id, readyJobId))
                .FirstAsync().ConfigureAwait(false);

                Assert.That(result?.Success, Is.False);
                Assert.That(jobWeExpectToHaveBeenAcknowledged.Acknowledgment, Is.Not.EqualTo(exampleAck));
                Assert.That(jobWeExpectToHaveBeenAcknowledged.Acknowledgment["RunnerId"], Is.Not.EqualTo(exampleAck["RunnerId"]));
            }).ConfigureAwait(false);
        }
    }
}