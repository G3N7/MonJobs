using System;
using System.Linq;
using System.Threading.Tasks;
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

            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "name", new JArray { "DeployApi" , "DeployWebsite" } }
                },
                AdhocQuery = "{\"$and\" : [{ \"Acknowledgment.superspecialfield\": \"superspecialfieldvalue\" }]}"
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
                        {"superspecialfield","superspecialfieldvalue"}
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

            var exampleQueueId = QueueId.Parse("ExampleQueue");
            var exampleQuery = new JobQuery
            {
                QueueId = exampleQueueId,
                HasAttributes = new JobAttributes
                {
                    { "name", new JArray { "DeployApi" , "DeployWebsite" } }
                },
                AdhocQuery = "{\"$or\" : [{ \"Acknowledgment.superspecialfield\": \"superspecialfieldvalue\" },{ \"Acknowledgment.superspecialfield\": \"superDuperSpecialFieldValue\" }]}"
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
                        {"superspecialfield","superDuperSpecialFieldValue"}
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
                        {"superspecialfield","superspecialfieldvalue"}
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

            var matchingJob1 = JobId.Generate();
            var matchingJob2 = JobId.Generate();
            var matchingJob3 = JobId.Generate();
            var unmatchedJob1 = JobId.Generate();

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
                        {"superspecialfield","superDuperSpecialFieldValue"}
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
                        {"superspecialfield","superspecialfieldvalue"}
                    }
                }
            };

            await RunInMongoLand(async database =>
            {
                var jobs = database.GetJobCollection();

                await jobs.InsertManyAsync(existingJobs).ConfigureAwait(false);

                var sut = new MongoJobQueryService(database);

                var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await sut.QueryFor(exampleQuery).ConfigureAwait(false);
                });      
                
                Assert.That(ex.Message,Does.Contain("Invalid adhocQuery"));

            }).ConfigureAwait(false);
        }
    }
}