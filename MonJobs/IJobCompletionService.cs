using System.Threading.Tasks;

namespace MonJobs
{
    /// <summary>
    /// Allows you to mark a job as complete.
    /// </summary>
    public interface IJobCompletionService
    {
        // doc: IJobCompletionService
        Task Complete(QueueId queueId, JobId jobId, JobResult result);
    }
}