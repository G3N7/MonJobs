using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonJobs
{
    public interface IJobQueryService
    {
        Task<IEnumerable<Job>> QueryFor(JobQuery query);
    }
}