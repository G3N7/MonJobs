using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonJobs.Http.ApiControllers
{
    public class JobsApiController : MonJobApiControllerBase
    {
        private readonly IJobQueryService _queryService;

        public JobsApiController(IJobQueryService queryService)
        {
            _queryService = queryService;
        }

        public virtual async Task<IEnumerable<Job>> Get(QueueId queueId, JobQuery query)
        {
            var queryResult = await _queryService.QueryFor(query);
            return queryResult;
        }

        // GET queue/7c9e6679742540de944be07fc1f90ae7/jobs/507f191e810c19729de860ea
        public virtual async Task<Job> Get(QueueId queueId, JobId id)
        {
            var byJobId = new JobQuery { JobIds = new[] { id }, Limit = 1 };
            var queryResult = await _queryService.QueryFor(byJobId);
            var foundJob = queryResult.FirstOrDefault();
            if (foundJob == null) NotFound();
            return foundJob;
        }

        // POST queue/7c9e6679742540de944be07fc1f90ae7/jobs
        public virtual Task<JobId> Post(QueueId queueId, JobAttributes value)
        {
            throw new NotImplementedException();
        }
    }
}
