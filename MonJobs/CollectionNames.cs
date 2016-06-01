using System.Net.Http.Headers;
using MongoDB.Driver;

namespace MonJobs
{
    public static class CollectionNames
    {
        public const string Job = "Job";

        public static IMongoCollection<Job> GetJobCollection(this IMongoDatabase database)
        {
            return database.GetCollection<Job>(Job);
        }
    }
}