using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonJobs
{
    public interface IJobCreationService
    {
        /// <summary>
        /// Allows you to create a new job in a queue
        /// </summary>
        /// <param name="queue">The queue to put the new job into</param>
        /// <param name="attributes">The attributes for this job</param>
        /// <returns>The newly created job's id</returns>
        Task<JobId> Create(QueueId queue, JobAttributes attributes);
    }
}
