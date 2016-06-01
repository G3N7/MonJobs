using MongoDB.Bson.Serialization;

namespace MonJobs.Serialization
{
    public static class MonJobsBsonConverters
    {
        public static void RegisterConverters()
        {
            BsonSerializer.RegisterSerializer(typeof(JobId), new JobIdBsonSerializer());
            BsonSerializer.RegisterSerializer(typeof(QueueId), new QueueIdBsonSerializer());
        }
    }
}