using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonJobs
{
    public interface IJobQueryCountService
    {
        // doc: IJobQueryCountService
        Task<long> QueryCount(JobQuery query);
    }
}