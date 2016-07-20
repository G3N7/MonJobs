using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using MonJobs.Http.ApiControllers;

namespace MonJobs.WebApi.Controllers
{
    [AllowAnonymous]
    public class JobsController : JobsApiController
    {
        public JobsController(IJobQueryService queryService, IJobCreationService creationService) : base(queryService, creationService)
        {
        }

        // GET queue/7c9e6679742540de944be07fc1f90ae7/jobs
        public override async Task<IEnumerable<Job>> Get(QueueId queueId, [FromBody]JobQuery query)
        {
            // todo: add auth
            return await base.Get(queueId, query);
        }

        // GET queue/7c9e6679742540de944be07fc1f90ae7/jobs/507f191e810c19729de860ea
        public override async Task<Job> Get(QueueId queueId, JobId id)
        {
            return await base.Get(queueId, id);
        }

        // POST queue/7c9e6679742540de944be07fc1f90ae7/jobs
        public override async Task<JobId> Post(QueueId queueId, [FromBody]JobAttributes value)
        {
            return await base.Post(queueId, value);
        }
    }

}