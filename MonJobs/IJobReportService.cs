using System.Threading.Tasks;

namespace MonJobs
{
    /// <summary>
    /// Allows you to constantly append results.
    /// </summary>
    public interface IJobReportService
    {
        // doc: IJobReportService
        Task AddReport(QueueId queue, JobId id, JobReport report);
    }
}