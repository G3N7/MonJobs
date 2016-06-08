using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MonJobs.Tests
{
    public class MongoJobQueryServiceTests : MongoTestBase
    {
        [Test]
        public async Task QueryFor_QueryByQueueIdOnly_YieldsAllResults()
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

                await jobs.InsertManyAsync(existingJobs);

                var sut = new MongoJobQuerySerivce(database);

                var results = (await sut.QueryFor(exampleQuery))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));
            });

        }

        [Test]
        public async Task QueryFor_QueryBySingleAttribute_YieldMatchingResults()
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

                await jobs.InsertManyAsync(existingJobs);

                var sut = new MongoJobQuerySerivce(database);

                var results = (await sut.QueryFor(exampleQuery))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            });
        }

        [Test]
        public async Task QueryFor_QueryByMultiAttributes_YieldMatchingResults()
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

                await jobs.InsertManyAsync(existingJobs);

                var sut = new MongoJobQuerySerivce(database);

                var results = (await sut.QueryFor(exampleQuery))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(1));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
            });
        }

        [Test]
        public async Task QueryFor_Limit_YieldsOnlyToTheLimit()
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

                await jobs.InsertManyAsync(existingJobs);

                var sut = new MongoJobQuerySerivce(database);

                var results = (await sut.QueryFor(exampleQuery))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));
            });
        }

        [Test]
        public async Task QueryFor_HasBeenAcknowledged_YieldsOnlyAcknowledgedJobs()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasBeenAcknowledged = true
            };

            var matchingJob1 = JobId.Generate();
            var matchingJob2 = JobId.Generate();
            var unmatchedJob1 = JobId.Generate();

            var existingJobs = new[] { new Job
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId,
                    Acknowledgment = new JobAcknowledgment
                    {
                        {"Result", "Failed" }
                    }
                }, new Job
                {
                    Id = matchingJob2,
                    QueueId = exampleQueueId,
                    Acknowledgment = new JobAcknowledgment
                    {
                        {"Result", "Success" }
                    }
                }, new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs);

                var sut = new MongoJobQuerySerivce(database);

                var results = (await sut.QueryFor(exampleQuery))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            });
        }

        [Test]
        public async Task QueryFor_HasNotBeenAcknowledged_YieldsOnlyJobsWhichHaveNotBeenAcknowledged()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasBeenAcknowledged = false
            };

            var matchingJob1 = JobId.Generate();
            var matchingJob2 = JobId.Generate();
            var unmatchedJob1 = JobId.Generate();

            var existingJobs = new[] { new Job
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId,
                }, new Job
                {
                    Id = matchingJob2,
                    QueueId = exampleQueueId,
                }, new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                    Acknowledgment = new JobAcknowledgment
                    {
                        {"Result", "Failed" }
                    }
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs);

                var sut = new MongoJobQuerySerivce(database);

                var results = (await sut.QueryFor(exampleQuery))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            });
        }

        [Test]
        public async Task QueryFor_HasResult_YieldsOnlyFinishedJobs()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasResult = true
            };

            var matchingJob1 = JobId.Generate();
            var matchingJob2 = JobId.Generate();
            var unmatchedJob1 = JobId.Generate();

            var existingJobs = new[] { new Job
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId,
                    Result = new JobResult
                    {
                        {"Result", "Failed" }
                    }
                }, new Job
                {
                    Id = matchingJob2,
                    QueueId = exampleQueueId,
                    Result = new JobResult
                    {
                        {"Result", "Success" }
                    }
                }, new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs);

                var sut = new MongoJobQuerySerivce(database);

                var results = (await sut.QueryFor(exampleQuery))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            });
        }

        [Test]
        public async Task QueryFor_HasNoResult_YieldsOnlyJobsWhichHaveNoResult()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasResult = false
            };

            var matchingJob1 = JobId.Generate();
            var matchingJob2 = JobId.Generate();
            var unmatchedJob1 = JobId.Generate();

            var existingJobs = new[] { new Job
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId,
                }, new Job
                {
                    Id = matchingJob2,
                    QueueId = exampleQueueId,
                }, new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                    Result = new JobResult
                    {
                        {"Result", "Failed" }
                    }
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs);

                var sut = new MongoJobQuerySerivce(database);

                var results = (await sut.QueryFor(exampleQuery))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            });
        }
    }

    public class MongoJobPeekNextServiceTests : MongoTestBase
    {
        [Test]
        public async Task PeekFor_NewJobs_ReturnsAvailableJobs()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new PeekNextQuery
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