using System.Threading.Tasks;
using MongoDB.Driver;
using MonJobs.Peek;

namespace MonJobs.Take
{
    public class MongoJobTakeNextService : IJobTakeNextService
    {
        private readonly IMongoCollection<Job> _jobs;

        public MongoJobTakeNextService(IMongoDatabase database)
        {
            _jobs = database.GetJobCollection();
        }

        public async Task<Job> TakeFor(TakeNextOptions options)
        {
            var jobQuery = new JobQuery
            {
                QueueId = options.QueueId,
                HasAttributes = options.HasAttributes,
                HasResult = false,
                HasBeenAcknowledged = false
            };

            var update = Builders<Job>.Update.Set(x => x.Acknowledgment, options.Acknowledgment);

            var result = await _jobs.FindOneAndUpdateAsync(jobQuery.BuildFilters(), update);

            return result;
        }
    }
}