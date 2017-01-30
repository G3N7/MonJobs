using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MonJobs
{
    public class MongoJobQueryCountService : IJobQueryCountService
    {
        private readonly IMongoCollection<Job> _jobs;

        public MongoJobQueryCountService(IMongoDatabase database)
        {
            _jobs = database.GetJobCollection();
        }

        public async Task<long> QueryCount(JobQuery query)
        {
            return await _jobs.Find(query.BuildFilters()).CountAsync().ConfigureAwait(false);
        }
    }
}