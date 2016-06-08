using System.Threading.Tasks;
using MonJobs.Peek;

namespace MonJobs.Take
{
    public interface IJobTakeNextService
    {
        Task<Job> TakeFor(TakeNextOptions options);
    }
}
