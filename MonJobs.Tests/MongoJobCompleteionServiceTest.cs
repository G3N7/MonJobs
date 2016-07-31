using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using NUnit.Framework;

namespace MonJobs.Tests
{
    internal class MongoJobCompleteionServiceTest : MongoTestBase
    {
        [Test]
        public async Task Complete_ValidResult_Persists()
        {
            var exampleQueueId = QueueId.Parse(Guid.NewGuid().ToString("N"));
            var exampleJobId = JobId.Generate();
            var exampleResult = new JobResult { { "Datacenter", "PEN" } };

            Assert.That(exampleResult, Is.Not.Null);

            await RunInMongoLand(async database =>
            {
                var collection = database.GetJobCollection();
                collection.InsertOne(new Job { QueueId = exampleQueueId, Id = exampleJobId });
                var sut = new MongoJobCompletionService(database);
                await sut.Complete(exampleQueueId, exampleJobId, exampleResult);

                var result = await collection.Find(Builders<Job>.Filter.Empty).SingleAsync();

                Assert.That(result.Result, Is.Not.Null);
                Assert.That(result.Result["Datacenter"], Is.EqualTo("PEN"));
            });

        }
    }
}