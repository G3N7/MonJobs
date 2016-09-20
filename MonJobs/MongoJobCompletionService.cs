using System.Threading.Tasks;
using MongoDB.Driver;

namespace MonJobs
{
    public class MongoJobCompletionService : IJobCompletionService
    {
        private readonly IMongoCollection<Job> _collection;

        public MongoJobCompletionService(IMongoDatabase database)
        {
            _collection = database.GetJobCollection();
        }

        public async Task Complete(QueueId queueId, JobId jobId, JobResult result)
        {
            var matchesQueueId = Builders<Job>.Filter.Eq(x => x.QueueId, queueId);
            var matchesJobId = Builders<Job>.Filter.Eq(x => x.Id, jobId);
            var matchesJobIdAndQueueId = Builders<Job>.Filter.And(matchesJobId, matchesQueueId);

            var update = Builders<Job>.Update.Set(x => x.Result, result);

            await _collection.UpdateOneAsync(matchesJobIdAndQueueId, update).ConfigureAwait(false);
        }
    }
}