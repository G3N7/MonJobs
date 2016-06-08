using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MonJobs
{
    public class MongoJobQuerySerivce : IJobQueryService
    {
        private readonly IMongoCollection<Job> _jobs;

        public MongoJobQuerySerivce(IMongoDatabase database)
        {
            _jobs = database.GetJobCollection();
        }

        public async Task<IEnumerable<Job>> QueryFor(JobQuery query)
        {
            var filters = new List<FilterDefinition<Job>>();
            var builder = Builders<Job>.Filter;

            filters.Add(builder.Eq(x => x.QueueId, query.QueueId));

            foreach (var attribute in query.HasAttributes)
            {
                var hasAttribute = builder.Eq(x => x.Attributes[attribute.Key], attribute.Value);
                filters.Add(hasAttribute);
            }

            var matchesAllFilters = Builders<Job>.Filter.And(filters);

            var mongoQuery = _jobs.Find(matchesAllFilters);

            if (query.Limit.HasValue) mongoQuery = mongoQuery.Limit(query.Limit.Value);

            return await mongoQuery.ToListAsync();
        }
    }
}