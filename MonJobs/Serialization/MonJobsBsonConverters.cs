using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MonJobs.Serialization
{
    public static class MonJobsBsonConverters
    {
        public static void RegisterConverters()
        {
            BsonSerializer.RegisterSerializer(typeof(JobId), new JobIdBsonSerializer());
            BsonSerializer.RegisterSerializer(typeof(QueueId), new QueueIdBsonSerializer());
            BsonSerializer.RegisterSerializer(typeof(JobAttributes), new DictionaryInterfaceImplementerSerializer<JobAttributes>());
            BsonSerializer.RegisterSerializer(typeof(JobAcknowledgment), new DictionaryInterfaceImplementerSerializer<JobAcknowledgment>());
            BsonSerializer.RegisterSerializer(typeof(JobReport), new DictionaryInterfaceImplementerSerializer<JobReport>());
            BsonSerializer.RegisterSerializer(typeof(JobResult), new DictionaryInterfaceImplementerSerializer<JobResult>());
        }
    }
}