using System.Collections.Generic;

namespace MonJobs
{
    // mitch: documentation
    public class JobAcknowledgment : Dictionary<string, object>
    {
        public JobAcknowledgment()
        {
        }

        public JobAcknowledgment(IDictionary<string, object> dictionary) : base(dictionary)
        {
        }
    }
}