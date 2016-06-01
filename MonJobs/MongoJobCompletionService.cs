using System.Threading.Tasks;
using MongoDB.Driver;

namespace MonJobs
{
    public class MongoJobCompletionService : IJobCompletionSerivce
    {
        private readonly IMongoCollection<Job> _collection;

        public MongoJobCompletionService(IMongoDatabase database)
        {
            _collection = database.GetJobCollection();
        }

        public async Task Complete(QueueId queueId, JobId jobId, JobResult result)
        {
            var matchsQueueId = Builders<Job>.Filter.Eq(x => x.QueueId, queueId);
            var matchsJobId = Builders<Job>.Filter.Eq(x => x.Id, jobId);
            var matchesJobIdAndQueueId = Builders<Job>.Filter.And(matchsJobId, matchsQueueId);

            var update = Builders<Job>.Update.Set(x => x.Result, result);

            await _collection.UpdateOneAsync(matchesJobIdAndQueueId, update);
        }
    }
}