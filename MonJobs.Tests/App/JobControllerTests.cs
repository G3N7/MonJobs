﻿using System;
using System.Linq;
using System.Threading.Tasks;
using MonJobs.Http.ApiControllers;
using Moq;
using NUnit.Framework;

namespace MonJobs.Tests.App
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

            var sut = new JobsApiController(mockQueryService.Object, null);
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

            var sut = new JobsApiController(mockQueryService.Object, null);
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

            var sut = new JobsApiController(mockQueryService.Object, null);
            var result = await sut.Get(exampleQueueId, new JobQuery
            {
                JobIds = new[] { exampleJobId }
            });

            var resultingIds = result.Select(x => x.Id);

            Assert.That(resultingIds, Contains.Item(exampleJobId));
        }

        [Test]
        public async Task Post_GivenJobAttributes_CreatesNewJob()
        {
            var exampleQueueId = QueueId.Parse(Guid.NewGuid().ToString("N"));
            var exampleAttributes = new JobAttributes
            {
                { "commandname", "ReleaseTheKrakin" }
            };

            var mockCreationService = new Mock<IJobCreationService>();
            var exampleCreatedJobId = JobId.Generate();
            mockCreationService.Setup(x => x.Create(exampleQueueId, exampleAttributes))
                .ReturnsAsync(exampleCreatedJobId);

            var sut = new JobsApiController(null, mockCreationService.Object);
            var result = await sut.Post(exampleQueueId, exampleAttributes);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.EqualTo(JobId.Empty()));
            Assert.That(result, Is.EqualTo(exampleCreatedJobId));
            mockCreationService.Verify(x => x.Create(It.IsAny<QueueId>(), It.IsAny<JobAttributes>()), Times.Once);
        }
    }
}