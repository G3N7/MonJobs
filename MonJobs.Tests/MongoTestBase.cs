using System;
using System.Linq;
using System.Threading.Tasks;
using Mongo2Go;
using MongoDB.Driver;
using MonJobs.Serialization;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MonJobs.Tests
{
    public class MongoTestBase
    {
        private static bool _hasRegistered;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            if (!_hasRegistered)
            {
                _hasRegistered = true;
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    Converters = MonJobsJsonConverters.TypesConverters.ToList()
                };
                MonJobsBsonConverters.RegisterConverters();
            }
        }

        public static async Task RunInMongoLand(Func<IMongoDatabase, Task> mongoWork)
        {
            using (var runner = MongoDbRunner.Start(System.IO.Path.GetTempPath()))
            {
                var server = new MongoClient(runner.ConnectionString);
                var database = server.GetDatabase("IntegrationTest");
                await mongoWork(database);
            }
        }
    }
}