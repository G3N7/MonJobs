using System.Collections.Generic;

namespace MonJobs
{
    // doc: JobResult
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