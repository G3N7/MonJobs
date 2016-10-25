using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonJobs.Tests.Performance
{
    abstract class ExistingJobsContext : JobDatabaseContext
    {
        protected abstract int NumberOfExistingCompletedJobs { get; }
        protected abstract int NumberOfExistingNewlyCreatedJobs { get; }
        protected abstract int NumberOfExistingAcknowledgedJobs { get; }
        protected abstract int NumberOfQueues { get; }
        protected IEnumerable<QueueId> GeneratedQueueId { get; private set; }

        protected virtual JobAttributes AttributeGenerator()
        {
            return new JobAttributes();
        }

        protected virtual JobAcknowledgment AckGenerator()
        {
            return new JobAcknowledgment();
        }

        protected virtual JobResult ResultGenerator()
        {
            return new JobResult();
        }

        protected override async Task EstablishContext()
        {
            await base.EstablishContext().ConfigureAwait(false);
            var queueIds = new List<QueueId>();

            for (int i = 0; i < NumberOfQueues; i++)
            {
                queueIds.Add(QueueId.Parse(Guid.NewGuid().ToString("N")));
            }

            var rando = new Random(DateTime.UtcNow.Millisecond);
            var jobsToInsert = new List<Job>();
            for (int i = 0; i < NumberOfExistingNewlyCreatedJobs; i++)
            {
                var queueIdToUse = queueIds[rando.Next(0, queueIds.Count)];
                var newJob = new Job
                {
                    Id = JobId.Generate(),
                    QueueId = queueIdToUse,
                    Attributes = AttributeGenerator()
                };
                jobsToInsert.Add(newJob);
            }

            for (int i = 0; i < NumberOfExistingAcknowledgedJobs; i++)
            {
                var queueIdToUse = queueIds[rando.Next(0, queueIds.Count)];
                var newJob = new Job
                {
                    Id = JobId.Generate(),
                    QueueId = queueIdToUse,
                    Attributes = AttributeGenerator(),
                    Acknowledgment = AckGenerator(),
                };
                jobsToInsert.Add(newJob);
            }

            for (int i = 0; i < NumberOfExistingCompletedJobs; i++)
            {
                var queueIdToUse = queueIds[rando.Next(0, queueIds.Count)];
                var newJob = new Job
                {
                    Id = JobId.Generate(),
                    QueueId = queueIdToUse,
                    Attributes = AttributeGenerator(),
                    Acknowledgment = AckGenerator(),
                    Result = ResultGenerator()
                };
                jobsToInsert.Add(newJob);
            }

            await Database.GetJobCollection().InsertManyAsync(jobsToInsert).ConfigureAwait(false);

            GeneratedQueueId = queueIds;
        }
    }
}