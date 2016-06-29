using System.Collections.Generic;

namespace MonJobs
{
    /// <summary>
    /// This is a named dictionary that represents the attributes of a job.
    /// </summary>
    /// <remarks>These can be used during consumption of the query APIs to select jobs that intrest you in that context.</remarks>
    public class JobAttributes : Dictionary<string, object>
    {
        public JobAttributes()
        {
        }

        public JobAttributes(IDictionary<string, object> dictionary) : base(dictionary)
        {
        }
    }
}