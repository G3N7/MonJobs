using System.Threading.Tasks;

namespace MonJobs
{
    /// <summary>
    /// Allows you to acknowledge ownership ownership of a job. (mitch: maybe array of checkout instead?)
    /// </summary>
    public interface IJobAcknowledgmentService
    {
        Task<AcknowledgementResult> Ack(QueueId queue, JobId id, JobAcknowledgment acknowledgment);
    }
}