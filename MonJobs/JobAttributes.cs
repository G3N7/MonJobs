using System.Collections.Generic;

namespace MonJobs
{
    // doc: JobAttributes
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