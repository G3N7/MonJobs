using System;
using System.Linq;
using System.Threading.Tasks;
using MonJobs.Http.ApiControllers;
using Moq;
using NUnit.Framework;

namespace MonJobs.Tests
{
    public class JobControllerTests
    {
        [Test]
        public async Task GetById_GivenValidIdForExistingJob_ReturnsJob()
        {
            var exampleQueueId = QueueId.Parse(Guid.NewGuid().ToString("N"));
            var exampleJobId = JobId.Generate();
            
            var exampleExistingJob = new Job
            {
                Id = exampleJobId,
            };

            var mockQueryService = new Mock<IJobQueryService>();
            mockQueryService
                .Setup(x => x.QueryFor(
                    It.Is<JobQuery>(query => query.JobIds.Contains(exampleJobId))))
                    .ReturnsAsync(new[] { exampleExistingJob });

            var sut = new JobsApiController(mockQueryService.Object);
            var result = await sut.Get(exampleQueueId, exampleJobId);

            Assert.That(result.Id, Is.EqualTo(exampleJobId));
        }

        [Test]
        public async Task GetById_GivenValidIdButNoJobThatItBelongsToo_Throws()
        {
            var exampleQueueId = QueueId.Parse(Guid.NewGuid().ToString("N"));
            var exampleJobId = JobId.Generate();
            var mockQueryService = new Mock<IJobQueryService>();
            mockQueryService
                .Setup(x => x.QueryFor(
                    It.Is<JobQuery>(query => query.JobIds.Contains(exampleJobId))))
                    .ReturnsAsync(Enumerable.Empty<Job>());

            var sut = new JobsApiController(mockQueryService.Object);
            var result = await sut.Get(exampleQueueId, exampleJobId);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByQuery_GivenValidQueryOptions_ReturnsResults()
        {
            var exampleQueueId = QueueId.Parse(Guid.NewGuid().ToString("N"));
            var exampleJobId = JobId.Generate();

            var exampleExistingJob = new Job
            {
                Id = exampleJobId,
            };

            var mockQueryService = new Mock<IJobQueryService>();
            mockQueryService
                .Setup(x => x.QueryFor(
                    It.Is<JobQuery>(query => query.JobIds.Contains(exampleJobId))))
                    .ReturnsAsync(new[] { exampleExistingJob });

            var sut = new JobsApiController(mockQueryService.Object);
            var result = await sut.Get(exampleQueueId, new JobQuery
            {
                JobIds = new[] {exampleJobId}
            });

            var resultingIds = result.Select(x => x.Id);
            
            Assert.That(resultingIds, Contains.Item(exampleJobId));
        }
    }
}
