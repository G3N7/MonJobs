using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonJobs.Peek
{
    public class MongoJobPeekNextService : IJobPeekNextService
    {
        private readonly IJobQueryService _jobQueryService;

        public MongoJobPeekNextService(IJobQueryService jobQueryService)
        {
            _jobQueryService = jobQueryService;
        }

        public Task<IEnumerable<Job>> PeekFor(PeekNextOptions options)
        {
            return _jobQueryService.QueryFor(new JobQuery
            {
                QueueId = options.QueueId,
                HasAttributes = options.HasAttributes,
                Limit = options.Limit,

                // We want to find results
                HasResult = false,
                HasBeenAcknowledged = false,
            });
        }
    }
}