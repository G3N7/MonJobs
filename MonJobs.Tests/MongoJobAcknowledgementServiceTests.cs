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
                await jobs.InsertManyAsync(existingJobs);

                var sut = new MongoJobAcknowledgmentService(database);
                await sut.Ack(exampleQueueName, readyJobId, exampleAck);

                var jobWeExpectToHaveBeenAcknowledged = await jobs.Find(Builders<Job>.Filter.Eq(x => x.Id, readyJobId)).FirstAsync();

                Assert.That(jobWeExpectToHaveBeenAcknowledged.Acknowledgment, Is.EqualTo(exampleAck));
            });
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
                await jobs.InsertManyAsync(existingJobs);

                var sut = new MongoJobAcknowledgmentService(database);
                await sut.Ack(exampleQueueName, readyJobId, exampleAck);

                var jobWeExpectToHaveBeenAcknowledged = await jobs.Find(Builders<Job>.Filter.Eq(x => x.Id, readyJobId)).FirstAsync();

                Assert.That(jobWeExpectToHaveBeenAcknowledged.Acknowledgment, Is.EqualTo(exampleAck));
            });
        }
    }
}