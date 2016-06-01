using System.Threading.Tasks;
using MongoDB.Driver;

namespace MonJobs
{
    public class MongoJobCreationService : IJobCreationService
    {
        private IMongoCollection<Job> _collection;

        public MongoJobCreationService(IMongoDatabase database)
        {
            _collection = database.GetCollection<Job>(CollectionNames.Job);
        }


        public async Task<JobId> Create(QueueId queue, JobAttributes attributes)
        {
            await _collection.InsertOneAsync(new Job { Id = JobId.Empty(), QueueId = queue, Attributes = attributes });
            return JobId.Empty();
        }
    }
}