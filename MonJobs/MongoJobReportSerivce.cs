using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MonJobs
{
    public class MongoJobReportSerivce : IJobReportSerivce
    {
        private readonly IMongoCollection<Job> _jobs;

        public MongoJobReportSerivce(IMongoDatabase database)
        {
            _jobs = database.GetJobCollection();
        }

        public async Task AddReport(QueueId queue, JobId id, JobReport report)
        {
            var builder = Builders<Job>.Filter;

            var matchesQueue = builder.Eq(x => x.QueueId, queue);
            var matchesId = builder.Eq(x => x.Id, id);

            var hasAcknowledgement = builder.Not(builder.Eq(x => x.Acknowledgment, null));

            var matchesQueueAndId = builder.And(matchesId, matchesQueue, hasAcknowledgement);

            var update = Builders<Job>.Update.Push(x => x.Reports, report);

            await _jobs.FindOneAndUpdateAsync(matchesQueueAndId, update);
        }
    }
}