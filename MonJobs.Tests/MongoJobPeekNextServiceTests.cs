using System;
using System.Linq;
using System.Threading.Tasks;
using MonJobs.Peek;
using NUnit.Framework;

namespace MonJobs.Tests
{
    public class MongoJobPeekNextServiceTests : MongoTestBase
    {
        [Test]
        public async Task PeekFor_NewJobs_ReturnsAvailableJobs()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new PeekNextOptions
            {
                QueueId = exampleQueueId
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

                await jobs.InsertManyAsync(existingJobs);

                var sut = new MongoJobPeekNextService(new MongoJobQuerySerivce(database));

                var results = (await sut.PeekFor(exampleQuery))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(1));

                Assert.That(results?.First()?.Id, Is.EqualTo(newlyCreatedJobId));
            });

        }
    }
}