using System;
using System.Linq;
using System.Threading.Tasks;
using MonJobs.Peek;
using NUnit.Framework;

namespace MonJobs.Tests
{
    public class SingleConsumerIntegrationTest : MongoTestBase
    {
        [Test]
        public async Task ProcessJobsForMyDataCenter_QueryThenAcknowledgeApproach()
        {
            var exampleQueueName = QueueId.Parse("ExampleQueue");

            await RunInMongoLand(async database =>
            {
                // Create two jobs that are relevant for my datacenter
                var creationService = new MongoJobCreationService(database);
                var spinWidgetJob = await creationService.Create(exampleQueueName, new JobAttributes
                {
                    { "Name", "SpinWidget" },
                    { "DataCenter", "CAL01" }
                });
                var fooBarJob = await creationService.Create(exampleQueueName, new JobAttributes
                {
                    { "Name", "FooBar" },
                    { "DataCenter", "CAL01" }
                });

                // todo: query for next available a job for my datacenter
                var peekNextService = new MongoJobPeekNextService(new MongoJobQuerySerivce(database));
                var peekQuery = new PeekNextQuery
                {
                    QueueId = exampleQueueName,
                    HasAttributes = new JobAttributes { { "DataCenter", "CAL01" } },
                    Limit = 1
                };
                var nextJob = (await peekNextService.PeekFor(peekQuery)).First();

                // todo: acknowledge the job
                var acknowledgementService = new MongoJobAcknowledgmentService(database);
                var standardAck = new JobAcknowledgment
                {
                    {"RunnerId", Guid.NewGuid().ToString("N") }
                };
                await acknowledgementService.Ack(exampleQueueName, nextJob.Id, standardAck);

                // todo: send reports
                Assert.Inconclusive();

                // todo: send result
                Assert.Inconclusive();

                // todo: query job status includes all relevant data.
                Assert.Inconclusive();
            });
        }
    }
}