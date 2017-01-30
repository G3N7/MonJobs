using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MonJobs.Tests
{
    internal class MongoJobQueryCountServiceTests : MongoTestBase
    {
        [Test]
        public async Task QueryCount_QueryByQueueIdOnly_YieldsAllResults()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId
            };

            var existingJobs = new[] { new Job
            {
                Id = JobId.Generate(),
                QueueId = exampleQueueId
            }, new Job
            {
                Id = JobId.Generate(),
                QueueId = exampleQueueId
            }, new Job
            {
                Id = JobId.Generate(),
                QueueId = QueueId.Parse(Guid.NewGuid().ToString("N"))
            } };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryCountService(database);

                var results = (await sut.QueryCount(exampleQuery).ConfigureAwait(false));

                long expected = 2;

                Assert.That(results, Is.Not.Null);
                Assert.AreEqual(results, expected);
            }).ConfigureAwait(false);

        }

        [Test]
        public async Task QueryCount_QueryBySingleAttribute_YieldMatchingResults()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "DataCenter", "CAL01"},
                }
            };

            var matchingJob1 = JobId.Generate();
            var matchingJob2 = JobId.Generate();
            var unmatchedJob1 = JobId.Generate();

            var existingJobs = new[] { new Job
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "DataCenter", "CAL01"},
                    }
                }, new Job
                {
                    Id = matchingJob2,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "DataCenter", "CAL01"},
                        { "Geo", "USA"},
                    }
                }, new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "Geo", "USA"},
                    }
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryCountService(database);

                var results = (await sut.QueryCount(exampleQuery).ConfigureAwait(false));

                long expected = 2;

                Assert.That(results, Is.Not.Null);
                Assert.AreEqual(results, expected);
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task QueryCount_QueryByMultiAttributes_YieldMatchingResults()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "DataCenter", "CAL01"},
                    { "Geo", "USA"}
                }
            };

            var matchingJob1 = JobId.Generate();
            var unmatchedJob1 = JobId.Generate();
            var unmatchedJob2 = JobId.Generate();

            var existingJobs = new[] { new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "DataCenter", "CAL01"},
                    }
                }, new Job
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "DataCenter", "CAL01"},
                        { "Geo", "USA"}
                    }
                }, new Job
                {
                    Id = unmatchedJob2,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "Geo", "USA"},
                    }
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryCountService(database);

                var results = (await sut.QueryCount(exampleQuery).ConfigureAwait(false));

                long expected = 1;

                Assert.That(results, Is.Not.Null);
                Assert.AreEqual(results, expected);
                
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task QueryCount_Limit_YieldsLimit()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                Limit = 2,
            };

            var existingJobs = new[] { new Job
            {
                Id = JobId.Generate(),
                QueueId = exampleQueueId
            }, new Job
            {
                Id = JobId.Generate(),
                QueueId = exampleQueueId
            }, new Job
            {
                Id = JobId.Generate(),
                QueueId = exampleQueueId
            } };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryCountService(database);

                var results = (await sut.QueryCount(exampleQuery).ConfigureAwait(false));

                long expected = 3;

                Assert.That(results, Is.Not.Null);
                Assert.AreEqual(results, expected);
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task QueryCount_Skip_YieldSkipped()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                Skip = 2,
            };

            var existingJobs = new[] { new Job
            {
                Id = JobId.Generate(),
                QueueId = exampleQueueId
            }, new Job
            {
                Id = JobId.Generate(),
                QueueId = exampleQueueId
            }, new Job
            {
                Id = JobId.Generate(),
                QueueId = exampleQueueId
            } };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryCountService(database);

                var results = (await sut.QueryCount(exampleQuery).ConfigureAwait(false));

                long expected = 3;

                Assert.That(results, Is.Not.Null);
                Assert.AreEqual(results, expected);
            }).ConfigureAwait(false);
        }
    }
}
