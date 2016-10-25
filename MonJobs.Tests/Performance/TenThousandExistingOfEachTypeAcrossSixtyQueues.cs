namespace MonJobs.Tests.Performance
{
    abstract class TenThousandExistingOfEachTypeAcrossSixtyQueues : ExistingJobsContext
    {
        protected override int NumberOfExistingCompletedJobs { get; } = 10000;
        protected override int NumberOfExistingNewlyCreatedJobs { get; } = 10000;
        protected override int NumberOfExistingAcknowledgedJobs { get; } = 10000;
        protected override int NumberOfQueues { get; } = 60;
    }
}