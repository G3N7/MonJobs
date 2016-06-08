using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonJobs
{
    public class MongoJobPeekNextService : IJobPeekNextService
    {
        private readonly IJobQueryService _jobQueryService;

        public MongoJobPeekNextService(IJobQueryService jobQueryService)
        {
            _jobQueryService = jobQueryService;
        }

        public Task<IEnumerable<Job>> PeekFor(PeekNextQuery query)
        {
            return _jobQueryService.QueryFor(new JobQuery
            {
                QueueId = query.QueueId,
                HasAttributes = query.HasAttributes,
                Limit = query.Limit,

                // We want to find results
                HasResult = false,
                HasBeenAcknowledged = false,
            });
        }
    }
}