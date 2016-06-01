using MongoDB.Bson;

namespace MonJobs.Serialization
{
    public class JobIdBsonSerializer : ToObjectIdBsonSerializer<JobId>
    {
        public override JobId CreateObjectFromObjectId(ObjectId serializedObj)
        {
            JobId parsedObj;
            JobId.TryParse(serializedObj, out parsedObj);
            return parsedObj;
        }
    }
}