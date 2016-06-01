using Newtonsoft.Json;

namespace MonJobs.Serialization
{
    public static class MonJobsJsonConverters
    {
        public static JsonConverter[] TypesConverters =
        {
            new JobIdJsonSerializer(), 
        };
    }
}