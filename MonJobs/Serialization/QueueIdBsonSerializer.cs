namespace MonJobs.Serialization
{
    public class QueueIdBsonSerializer : ToStringBsonSerializer<QueueId>
    {
        public override QueueId CreateObjectFromString(string serializedObj)
        {
            QueueId parsedObj;
            QueueId.TryParse(serializedObj, out parsedObj);
            return parsedObj;
        }
    }
}