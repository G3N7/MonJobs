using System;
using System.Threading.Tasks;
using Mongo2Go;
using MongoDB.Driver;

namespace MonJobs.Tests
{
    abstract class JobDatabaseContext : ContextSpecification
    {
        private MongoDbRunner _runner;
        protected IMongoDatabase Database;
        const string DatabaseName = "IntegrationTest";

        protected override async Task EstablishContext()
        {
            await base.EstablishContext().ConfigureAwait(false);
            var binSearchPattern = Environment.GetEnvironmentVariable("BIN_SEARCH_PATTERN") ?? @"tools\mongodb-win32*\bin";
            _runner = MongoDbRunner.Start(System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N"), binSearchPattern);
            var server = new MongoClient(_runner.ConnectionString);
            Database = server.GetDatabase(DatabaseName);
            await Database.Client.DropDatabaseAsync(DatabaseName).ConfigureAwait(false);
        }

        public override async Task AfterAll()
        {
            await Database.Client.DropDatabaseAsync(DatabaseName).ConfigureAwait(false);
            if (!_runner.Disposed) _runner.Dispose();
            await base.AfterAll().ConfigureAwait(false);
        }
    }
}