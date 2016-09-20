using System.Threading.Tasks;
using NUnit.Framework;

namespace MonJobs.Tests
{
    internal class SingleConsumerIntegrationTests : MongoUsecaseIntegrationTestBase
    {
        [Test]
        public async Task ProcessJobsForMyDataCenter_TwoInQueue_QueryThenAcknowledgeApproach()
        {
            var exampleQueueName = QueueId.Parse("ExampleQueue");

            await RunInMongoLand(async database =>
            {
                // Create two jobs that are relevant for my datacenter
                var creationService = new MongoJobCreationService(database);
                var spinWidgetJobId = await creationService.Create(exampleQueueName, new JobAttributes
                {
                    { "Name", "SpinWidget" },
                    { "DataCenter", "CAL01" }
                }).ConfigureAwait(false);
                var fooBarJobId = await creationService.Create(exampleQueueName, new JobAttributes
                {
                    { "Name", "FooBar" },
                    { "DataCenter", "CAL01" }
                }).ConfigureAwait(false);

                var finished1 = await TryProcessOneJobUsingPeekThanAck(database, exampleQueueName, new JobAttributes { { "DataCenter", "CAL01" } }).ConfigureAwait(false);
                var finished2 = await TryProcessOneJobUsingPeekThanAck(database, exampleQueueName, new JobAttributes { { "DataCenter", "CAL01" } }).ConfigureAwait(false);
                var finished3 = await TryProcessOneJobUsingPeekThanAck(database, exampleQueueName, new JobAttributes { { "DataCenter", "CAL01" } }).ConfigureAwait(false);

                Assert.That(finished1, Is.Not.Null);
                Assert.That(finished2, Is.Not.Null);
                Assert.That(finished3, Is.Null);
            }).ConfigureAwait(false);
        }

        [Test]
        public async Task ProcessJobsForMyDataCenter_TwoInQueue_TakeNextApproach()
        {
            var exampleQueueName = QueueId.Parse("ExampleQueue");

            await RunInMongoLand(async database =>
            {
                // Create two jobs that are relevant for my datacenter
                var creationService = new MongoJobCreationService(database);
                var spinWidgetJobId = await creationService.Create(exampleQueueName, new JobAttributes
                {
                    { "Name", "SpinWidget" },
                    { "DataCenter", "CAL01" }
                }).ConfigureAwait(false);
                var fooBarJobId = await creationService.Create(exampleQueueName, new JobAttributes
                {
                    { "Name", "FooBar" },
                    { "DataCenter", "CAL01" }
                }).ConfigureAwait(false);

                var myDataCenter = new JobAttributes { { "DataCenter", "CAL01" } };

                var finished1 = await TryProcessOneJobUsingTakeNext(database, exampleQueueName, myDataCenter).ConfigureAwait(false);
                var finished2 = await TryProcessOneJobUsingTakeNext(database, exampleQueueName, myDataCenter).ConfigureAwait(false);
                var finished3 = await TryProcessOneJobUsingTakeNext(database, exampleQueueName, myDataCenter).ConfigureAwait(false);

                Assert.That(finished1, Is.Not.Null);
                Assert.That(finished2, Is.Not.Null);
                Assert.That(finished3, Is.Null);
            }).ConfigureAwait(false);
        }
    }
}
