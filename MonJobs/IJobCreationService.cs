using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonJobs
{
    /// <summary>
    /// Allows you to create a new job
    /// </summary>
    public interface IJobCreationService
    {
        Task<JobId> Create(QueueId queue, JobAttributes attributes);
    }
}
