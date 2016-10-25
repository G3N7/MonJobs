namespace MonJobs.Tests.Performance
{
    abstract class OneHundredThousandExistingOfEachTypeAcrossSixtyQueues : ExistingJobsContext
    {
        protected override int NumberOfExistingCompletedJobs { get; } = 100000;
        protected override int NumberOfExistingNewlyCreatedJobs { get; } = 100000;
        protected override int NumberOfExistingAcknowledgedJobs { get; } = 100000;
        protected override int NumberOfQueues { get; } = 60;
    }
}