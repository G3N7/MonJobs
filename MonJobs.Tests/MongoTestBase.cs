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
            var connectionString = Environment.GetEnvironmentVariable("MONGO_DB_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                using (var runner = MongoDbRunner.Start(System.IO.Path.GetTempPath()))
                {
                    await RunAction(runner.ConnectionString, mongoWork);
                }
            }
            else
            {
                await RunAction(connectionString, mongoWork);
            }
        }

        private static async Task RunAction(string connectionString, Func<IMongoDatabase, Task> mongoWork)
        {
            var server = new MongoClient(connectionString);
            var database = server.GetDatabase("IntegrationTest");
            var collections = await database.ListCollections().ToListAsync();
            foreach (var collection in collections)
            {
                foreach (var name in collection.Names)
                {
                    await database.DropCollectionAsync(name);
                }
            }
            await mongoWork(database);
        }
    }
}