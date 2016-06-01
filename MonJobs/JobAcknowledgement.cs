using System.Collections.Generic;

namespace MonJobs
{
    // mitch: documentation
    public class JobAcknowledgement : Dictionary<string, object>
    {
        public JobAcknowledgement()
        {
        }

        public JobAcknowledgement(IDictionary<string, object> dictionary) : base(dictionary)
        {
        }
    }
}