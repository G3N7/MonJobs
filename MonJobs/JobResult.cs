using System.Collections.Generic;

namespace MonJobs
{
    /// <summary>
    /// This is a named dictionary that represents the final result of a job.
    /// </summary>
    public class JobResult : Dictionary<string, object>
    {
        public JobResult()
        {
        }

        public JobResult(IDictionary<string, object> dictionary) : base(dictionary)
        {
        }
    }
}