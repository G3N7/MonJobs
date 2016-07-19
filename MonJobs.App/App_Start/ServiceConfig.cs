using LAN.Core.DependencyInjection;
using MongoDB.Driver;

namespace MonJobs.App
{
    // ReSharper disable once InconsistentNaming
    public class ServiceConfig
    {
        public static void Configure(IContainer container, IMongoDatabase database)
        {
            container.RegisterSingleton(database);
            container.Bind<IJobQueryService, MongoJobQueryService>(true);
        }
    }
}