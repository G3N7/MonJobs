using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonJobs
{
    // mitch: documentation
    public class Job
    {
        public JobId Id { get; set; }
        public QueueId QueueId { get; set; }
        public JobAttributes Attributes { get; set; }
        public JobAcknowledgment[] Acknowledgment { get; set; }
        public JobReport[] Reports { get; set; }
        public JobResult Result { get; set; }
    }
}
