using System.Collections.Generic;

namespace MonJobs
{
    // mitch: documentation
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