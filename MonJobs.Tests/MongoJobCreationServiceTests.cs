using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace MonJobs.Tests
{
    public class MongoJobCreationServiceTests : MongoTestBase
    {
        [Test]
        public async Task Create_ValidInitialData_Persists()
        {
            var exampleQueueId = QueueId.Parse(Guid.NewGuid().ToString("N"));
            var exampleAttributes = new { name = "Thing ToDo" }.ToExpando().ToJobAttributes();

            await RunInMongoLand(async database =>
            {
                var sut = new MongoJobCreationService(database);

                var newId = await sut.Create(exampleQueueId, exampleAttributes);

                Assert.That(newId, Is.Not.Null);
                var collection = database.GetCollection<Job>(CollectionNames.Job);
                var newlyCreatedJob = await collection.Find(Builders<Job>.Filter.Empty).SingleAsync();

                Assert.That(newlyCreatedJob.Id, Is.Not.Null);
                Assert.That(newlyCreatedJob.Id, Is.Not.EqualTo(JobId.Empty()));
                Assert.That(newlyCreatedJob.QueueId, Is.EqualTo(exampleQueueId));
                Assert.That(newlyCreatedJob.Attributes, Is.EqualTo(exampleAttributes));
            });
        }
    }
}