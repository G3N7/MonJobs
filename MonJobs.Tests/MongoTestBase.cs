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
    internal class MongoTestBase
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

        protected static async Task RunInMongoLand(Func<IMongoDatabase, Task> mongoWork)
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGO_DB_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(connectionString))
            {

                using (var runner = MongoDbRunner.Start(System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N")))
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
            const string databaseName = "IntegrationTest";
            var database = server.GetDatabase(databaseName);
            await database.Client.DropDatabaseAsync(databaseName);
            await mongoWork(database);
            await database.Client.DropDatabaseAsync(databaseName);
        }
    }
}