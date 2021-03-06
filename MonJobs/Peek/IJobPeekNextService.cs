﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonJobs.Peek
{
    /// <summary>
    /// Allows you to peek at the next N jobs for a given queue.
    /// </summary>
    public interface IJobPeekNextService
    {
        Task<IEnumerable<Job>> PeekFor(PeekNextOptions options);
    }
}