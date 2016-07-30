using System;
using System.Threading.Tasks;
using Mongo2Go;
using MongoDB.Driver;

namespace MonJobs.Tests
{
    internal abstract class MongoTestBase : TestBase
    {
        protected static async Task RunInMongoLand(Func<IMongoDatabase, Task> mongoWork)
        {
            var binSearchPattern = Environment.GetEnvironmentVariable("BIN_SEARCH_PATTERN") ?? @"tools\mongodb-win32*\bin";
            using (var runner = MongoDbRunner.Start(System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N"), binSearchPattern))
            {
                await RunAction(runner.ConnectionString, mongoWork);
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