using System.Collections.Generic;

namespace MonJobs
{
    /// <summary>
    /// This is a named dictionary that represents progress information about the job.
    /// </summary>
    public class JobReport : Dictionary<string, object>
    {
        public JobReport()
        {
        }

        public JobReport(IDictionary<string, object> dictionary) : base(dictionary)
        {
        }
    }
}