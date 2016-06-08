using System.Threading.Tasks;
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
                Assert.Inconclusive();

                // todo: acknowledge the job
                Assert.Inconclusive();

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