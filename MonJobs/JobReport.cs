using System.Collections.Generic;

namespace MonJobs
{
    // mitch: documentation
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