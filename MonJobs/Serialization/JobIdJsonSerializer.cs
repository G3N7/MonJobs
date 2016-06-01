namespace MonJobs.Serialization
{
    public class JobIdJsonSerializer : ToStringJsonSerializer<JobId>
    {
        public override JobId CreateObjectFromString(string serializedObj)
        {
            JobId parsedObj;
            JobId.TryParse(serializedObj, out parsedObj);
            return parsedObj;
        }
    }
}