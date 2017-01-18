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
    internal class MongoJobQueryServiceTests : MongoTestBase
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

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));
            }).ConfigureAwait(false);

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

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            }).ConfigureAwait(false);
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

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(1));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
            }).ConfigureAwait(false);
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

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task QueryFor_Skip_YieldsOnlyWhatIsAfterTheSkipped()
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

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(1));
            }).ConfigureAwait(false);
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

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            }).ConfigureAwait(false);
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

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            }).ConfigureAwait(false);
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

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            }).ConfigureAwait(false);
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

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task QueryFor_JobIds_YieldsOnlyJobsWithThoseIds()
        {

            var matchingJob1 = JobId.Generate();
            var matchingJob2 = JobId.Generate();
            var unmatchedJob1 = JobId.Generate();

            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                JobIds = new[] { matchingJob1, matchingJob2 }
            };

            var existingJobs = new[] { new Job
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId
                }, new Job
                {
                    Id = matchingJob2,
                    QueueId = exampleQueueId
                }, new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task QueryFor_AnArrayOfValues_ReturnsAllJobsMatching()
        {

            var matchingJob1 = JobId.Generate();
            var matchingJob2 = JobId.Generate();
            var unmatchedJob1 = JobId.Generate();

            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "name", new [] { "DeployApi" , "DeployWebsite" } }
                }
            };

            var existingJobs = new[] { new Job
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployApi" }
                    }
                }, new Job
                {
                    Id = matchingJob2,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployWebsite" }
                    }
                }, new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeploySchema" }
                    }
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task QueryFor_AJArrayOfStrings_ReturnsAllJobsMatching()
        {

            var matchingJob1 = JobId.Generate();
            var matchingJob2 = JobId.Generate();
            var unmatchedJob1 = JobId.Generate();

            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "name", new JArray { "DeployApi" , "DeployWebsite" } }
                }
            };

            var existingJobs = new[] { new Job
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployApi" }
                    }
                }, new Job
                {
                    Id = matchingJob2,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployWebsite" }
                    }
                }, new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeploySchema" }
                    }
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task QueryFor_AJArrayOfIntegers_ReturnsAllJobsMatching()
        {

            var matchingJob1 = JobId.Generate();
            var matchingJob2 = JobId.Generate();
            var unmatchedJob1 = JobId.Generate();

            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "cpu", new JArray { 1 , 7 } }
                }
            };

            var existingJobs = new[] { new Job
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "cpu", 1 }
                    }
                }, new Job
                {
                    Id = matchingJob2,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "cpu", 7 }
                    }
                }, new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "cpu", 8 }
                    }
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
                Assert.That(foundIds, Contains.Item(matchingJob2));
            }).ConfigureAwait(false);
        }


        [Test]
        public async Task QueryFor_AJArrayOfStringsAndWithSimpleAdhocQuery_ReturnsAllJobsMatching()
        {

            var matchingJob1 = JobId.Generate();
            var matchingJob2 = JobId.Generate();
            var matchingJob3 = JobId.Generate();
            var unmatchedJob1 = JobId.Generate();

            var exampleAcknowledgedDateTime1 = new DateTime(2010, 1, 22, 22, 00, 00, DateTimeKind.Utc);
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "name", new JArray { "DeployApi" , "DeployWebsite" } }
                },
                AdhocQuery = "{\"$and\" : [{ \"Acknowledgment.acknowledgedDateTime\": ISODate(\"2010-01-22T22:00:00.000Z\")}]}"
            };

            var existingJobs = new[] { new Job
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployApi" }
                    }
                }, new Job
                {
                    Id = matchingJob2,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployWebsite" }
                    }
                }, new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeploySchema" }
                    }
                }, new Job()
                {
                    Id = matchingJob3,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployApi" }
                    },
                    Acknowledgment = new JobAcknowledgment
                    {
                        {"acknowledgedDateTime",exampleAcknowledgedDateTime1}
                    }
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(1));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob3));
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task QueryFor_AJArrayOfStringsAndWithOrStyleAdhocQuery_ReturnsAllJobsMatching()
        {

            var matchingJob1 = JobId.Generate();
            var matchingJob2 = JobId.Generate();
            var matchingJob3 = JobId.Generate();
            var unmatchedJob1 = JobId.Generate();

            var exampleAcknowledgedDateTime1 = new DateTime(2010, 1, 22, 22, 00, 00, DateTimeKind.Utc);
            var exampleAcknowledgedDateTime2 = new DateTime(2010, 1, 22, 23, 00, 00, DateTimeKind.Utc);
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "name", new JArray { "DeployApi" , "DeployWebsite" } }
                },
                AdhocQuery = "{\"$or\" : [{ \"Acknowledgment.acknowledgedDateTime\": ISODate(\"2010-01-22T22:00:00.000Z\")},{ \"Acknowledgment.acknowledgedDateTime\": ISODate(\"2010-01-22T23:00:00.000Z\")}]}"
            };

            var existingJobs = new[] { new Job
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployApi" }
                    }
                }, new Job
                {
                    Id = matchingJob2,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployWebsite" }
                    },
                    Acknowledgment = new JobAcknowledgment
                    {
                        {"acknowledgedDateTime",exampleAcknowledgedDateTime1}
                    }
                }, new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeploySchema" }
                    }
                }, new Job()
                {
                    Id = matchingJob3,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployApi" }
                    },
                    Acknowledgment = new JobAcknowledgment
                    {
                        {"acknowledgedDateTime",exampleAcknowledgedDateTime2}
                    }
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob2));
                Assert.That(foundIds, Contains.Item(matchingJob3));
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task QueryFor_AJArrayOfStringsAndWithInvalidAdhocQuery_ThrowsException()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "name", new JArray { "DeployApi" , "DeployWebsite" } }
                },
                AdhocQuery = "SampeInvalidMongoQuery"
            };

            await RunInMongoLand(database =>
            {
                var sut = new MongoJobQueryService(database);

                var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await sut.QueryFor(exampleQuery).ConfigureAwait(false);
                });

                Assert.That(ex.Message, Does.Contain("Invalid adhocQuery"));
                return Task.FromResult(true);
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task QueryFor_AJArrayOfStringsAndWithSimpleAdhocSort_ReturnsAllJobsMatchingSorted()
        {

            var jobOrder1 = JobId.Generate();
            var jobOrder2 = JobId.Generate();
            var jobOrder3 = JobId.Generate();
            var jobOrder4 = JobId.Generate();

            var exampleAcknowledgedDateTime1 = new DateTime(2010, 1, 22, 22, 00, 00, DateTimeKind.Utc);
            var exampleAcknowledgedDateTime2 = new DateTime(2009, 1, 22, 22, 00, 00, DateTimeKind.Utc);
            var exampleAcknowledgedDateTime3 = new DateTime(2008, 1, 22, 22, 00, 00, DateTimeKind.Utc);
            var exampleAcknowledgedDateTime4 = new DateTime(2011, 1, 22, 22, 00, 00, DateTimeKind.Utc);

            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "name", new JArray { "DeployApi" , "DeployWebsite" } }
                },
                AdhocSort = "{\"Acknowledgment.acknowledgedDateTime\": -1}"
            };

            var existingJobs = new[] { new Job
                {
                    Id = jobOrder1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployApi" }
                    },
                    Acknowledgment = new JobAcknowledgment
                    {
                        {"acknowledgedDateTime",exampleAcknowledgedDateTime1}
                    }
                }, new Job
                {
                    Id = jobOrder2,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployWebsite" }
                    },
                    Acknowledgment = new JobAcknowledgment
                    {
                        {"acknowledgedDateTime",exampleAcknowledgedDateTime2}
                    }
                }, new Job
                {
                    Id = jobOrder3,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeploySchema" }
                    },
                    Acknowledgment = new JobAcknowledgment
                    {
                        {"acknowledgedDateTime",exampleAcknowledgedDateTime3}
                    }
                }, new Job
                {
                    Id = jobOrder4,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployWebsite" }
                    },
                    Acknowledgment = new JobAcknowledgment
                    {
                        {"acknowledgedDateTime",exampleAcknowledgedDateTime4}
                    }
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(3));
                
                var foundIds = results.Select(x => x.Id).ToList();
                List<MonJobs.JobId> expecedIds = new List<MonJobs.JobId>();
                expecedIds.Add(jobOrder4);
                expecedIds.Add(jobOrder1);
                expecedIds.Add(jobOrder2);

                Assert.AreEqual(foundIds, expecedIds);
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task QueryFor_AJArrayOfStringsAndWithInvalidAdhocSort_ThrowsException()
        {
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "name", new JArray { "DeployApi" , "DeployWebsite" } }
                },
                AdhocSort = "SampeInvalidMongoSort"
            };

            await RunInMongoLand(database =>
            {
                var sut = new MongoJobQueryService(database);

                var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await sut.QueryFor(exampleQuery).ConfigureAwait(false);
                });

                Assert.That(ex.Message, Does.Contain("Invalid adhocSort"));
                return Task.FromResult(true);
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task QueryFor_AJArrayOfStringsAndWithFilter_ReturnsAllJobsMatching()
        {

            var unmatchedJob1 = JobId.Generate();
            var unmatchedJob2 = JobId.Generate();
            var unmatchedJob3 = JobId.Generate();
            var matchingJob1 = JobId.Generate();


            var exampleAcknowledgedDateTime1 = new DateTime(2010, 1, 22, 22, 00, 00, DateTimeKind.Utc);
            var exampleQueueId = QueueId.Parse("ExampleQueue");
            FilterDefinition<BsonDocument> filter = new BsonDocument("Attributes.name", "DeployWebsite");
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<BsonDocument>();

            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "name", new JArray { "DeployApi" , "DeployWebsite" } }
                },
                AdhocFilter = filter.Render(documentSerializer, serializerRegistry)
        };

            var existingJobs = new[] { new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployApi" }
                    }
                }, new Job
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployWebsite" }
                    }
                }, new Job
                {
                    Id = unmatchedJob2,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeploySchema" }
                    }
                }, new Job()
                {
                    Id = unmatchedJob3,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployApi" }
                    },
                    Acknowledgment = new JobAcknowledgment
                    {
                        {"acknowledgedDateTime",exampleAcknowledgedDateTime1}
                    }
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(1));
                
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
            }).ConfigureAwait(false);
        }

        public async Task QueryFor_AJArrayOfStringsAndWithFilterFromString_ReturnsAllJobsMatching()
        {

            var unmatchedJob1 = JobId.Generate();
            var unmatchedJob2 = JobId.Generate();
            var unmatchedJob3 = JobId.Generate();
            var matchingJob1 = JobId.Generate();


            var exampleAcknowledgedDateTime1 = new DateTime(2010, 1, 22, 22, 00, 00, DateTimeKind.Utc);
            var exampleQueueId = QueueId.Parse("ExampleQueue");

            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "name", new JArray { "DeployApi" , "DeployWebsite" } }
                },
                AdhocFilter = BsonSerializer.Deserialize<BsonDocument>("{\"$and\" : [{ \"Acknowledgment.acknowledgedDateTime\": ISODate(\"2010-01-22T22:00:00.000Z\")}]}")
            };

            var existingJobs = new[] { new Job
                {
                    Id = unmatchedJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployApi" }
                    }
                }, new Job
                {
                    Id = unmatchedJob2,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployWebsite" }
                    }
                }, new Job
                {
                    Id = unmatchedJob3,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeploySchema" }
                    }
                }, new Job()
                {
                    Id = matchingJob1,
                    QueueId = exampleQueueId,
                    Attributes = new JobAttributes
                    {
                        { "name", "DeployApi" }
                    },
                    Acknowledgment = new JobAcknowledgment
                    {
                        {"acknowledgedDateTime",exampleAcknowledgedDateTime1}
                    }
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var results = (await sut.QueryFor(exampleQuery).ConfigureAwait(false))?.ToList();

                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(1));

                // ReSharper disable once AssignNullToNotNullAttribute
                var foundIds = results.Select(x => x.Id).ToList();

                Assert.That(foundIds, Contains.Item(matchingJob1));
            }).ConfigureAwait(false);
        }
    }
}
