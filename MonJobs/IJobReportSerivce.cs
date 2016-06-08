using System.Threading.Tasks;

namespace MonJobs
{
    /// <summary>
    /// Allows you to constantly append results.
    /// </summary>
    public interface IJobReportSerivce
    {
        Task AddReport(QueueId queue, JobId id, JobReport report);
    }
}