using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MonJobs
{
    public class MongoJobQueryService : IJobQueryService
    {
        private readonly IMongoCollection<Job> _jobs;

        public MongoJobQueryService(IMongoDatabase database)
        {
            _jobs = database.GetJobCollection();
        }

        public async Task<IEnumerable<Job>> QueryFor(JobQuery query)
        {
            var mongoQuery = _jobs.Find(query.BuildFilters());

            if (query.Limit.HasValue) mongoQuery = mongoQuery.Limit(query.Limit.Value);

            return await mongoQuery.ToListAsync().ConfigureAwait(false);
        }
    }
}