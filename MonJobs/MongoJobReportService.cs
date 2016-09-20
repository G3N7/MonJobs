using System.Threading.Tasks;
using MongoDB.Driver;

namespace MonJobs
{
    public class MongoJobReportService : IJobReportService
    {
        private readonly IMongoCollection<Job> _jobs;

        public MongoJobReportService(IMongoDatabase database)
        {
            _jobs = database.GetJobCollection();
        }

        public async Task AddReport(QueueId queue, JobId id, JobReport report)
        {
            var builder = Builders<Job>.Filter;

            var matchesQueue = builder.Eq(x => x.QueueId, queue);
            var matchesId = builder.Eq(x => x.Id, id);

            var hasAcknowledgment = builder.Not(builder.Eq(x => x.Acknowledgment, null));

            var matchesQueueAndId = builder.And(matchesId, matchesQueue, hasAcknowledgment);

            var update = Builders<Job>.Update.Push(x => x.Reports, report);

            await _jobs.FindOneAndUpdateAsync(matchesQueueAndId, update).ConfigureAwait(false);
        }
    }
}