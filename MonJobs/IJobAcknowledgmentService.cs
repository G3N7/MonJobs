using System.Threading.Tasks;

namespace MonJobs
{
    public interface IJobAcknowledgmentService
    {
        /// <summary>
        /// Allows you to acknowledge ownership ownership of a job, if it has not yet been acknowledged by something else
        /// </summary>
        /// <param name="queue">The queue where the job exists</param>
        /// <param name="id">The unique id of the job</param>
        /// <param name="acknowledgment">The acknowledgment you would like for the job to be stamped with if it is available</param>
        /// <returns>A promise of if we got the job or something else did before us</returns>
        Task<AcknowledgmentResult> Ack(QueueId queue, JobId id, JobAcknowledgment acknowledgment);
    }
}