using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

namespace MonJobs.WebApi.Controllers
{
    [AllowAnonymous]
    public class JobsController : ApiController
    {
        private readonly IJobQueryService _queryService;
        private readonly IJobCreationService _creationService;

        public JobsController(IJobQueryService queryService, IJobCreationService creationService)
        {
            _queryService = queryService;
            _creationService = creationService;
        }

        // GET queue/7c9e6679742540de944be07fc1f90ae7/jobs
        public async Task<IEnumerable<Job>> Get(string queueId)
        {
            // todo: add auth
            var content = await Request.Content.ReadAsStringAsync();

            var query = string.IsNullOrWhiteSpace(content)
                ? new JobQuery()
                : JsonConvert.DeserializeObject<JobQuery>(content);

            query.QueueId = QueueId.Parse(queueId);

            var queryResult = await _queryService.QueryFor(query);
            return queryResult;
        }

        // GET queue/7c9e6679742540de944be07fc1f90ae7/jobs/507f191e810c19729de860ea
        public async Task<Job> Get(string queueId, string id)
        {
            var byJobId = new JobQuery { QueueId = QueueId.Parse(queueId), JobIds = new[] { JobId.Parse(id) }, Limit = 1 };
            var queryResult = await _queryService.QueryFor(byJobId);
            var foundJob = queryResult.FirstOrDefault();
            if (foundJob == null) NotFound();
            return foundJob;
        }

        // POST queue/7c9e6679742540de944be07fc1f90ae7/jobs
        public async Task<JobId> Post(string queueId, [FromBody]JobAttributes value)
        {
            return await _creationService.Create(QueueId.Parse(queueId), value);
        }
    }

}