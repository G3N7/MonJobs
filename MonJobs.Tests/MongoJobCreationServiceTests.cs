﻿using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using NUnit.Framework;

namespace MonJobs.Tests
{
    internal class MongoJobCreationServiceTests : MongoTestBase
    {
        [Test]
        public async Task Create_ValidInitialData_Persists()
        {
            var exampleQueueId = QueueId.Parse(Guid.NewGuid().ToString("N"));
            var exampleAttributes = new JobAttributes { { "name", "Thing ToDo" } };

            await RunInMongoLand(async database =>
            {
                var sut = new MongoJobCreationService(database);

                var newId = await sut.Create(exampleQueueId, exampleAttributes).ConfigureAwait(false);

                Assert.That(newId, Is.Not.Null);
                var collection = database.GetCollection<Job>(CollectionNames.Job);
                var newlyCreatedJob = await collection.Find(Builders<Job>.Filter.Empty).SingleAsync().ConfigureAwait(false);

                Assert.That(newlyCreatedJob.Id, Is.Not.Null);
                Assert.That(newlyCreatedJob.Id, Is.Not.EqualTo(JobId.Empty()));
                Assert.That(newlyCreatedJob.QueueId, Is.EqualTo(exampleQueueId));
                Assert.That(newlyCreatedJob.Attributes, Is.EqualTo(exampleAttributes));
                Assert.That(newlyCreatedJob.Attributes["name"], Is.EqualTo("Thing ToDo"));
            }).ConfigureAwait(false);
        }
    }
}