using System.Threading.Tasks;
using MongoDB.Driver;

namespace MonJobs
{
    public class MongoJobCreationService : IJobCreationService
    {
        private readonly IMongoCollection<Job> _collection;

        public MongoJobCreationService(IMongoDatabase database)
        {
            _collection = database.GetJobCollection();
        }

        public async Task<JobId> Create(QueueId queue, JobAttributes attributes)
        {
            var newId = JobId.Generate();
            await _collection.InsertOneAsync(new Job { Id = newId, QueueId = queue, Attributes = attributes });
            return newId;
        }
    }
}