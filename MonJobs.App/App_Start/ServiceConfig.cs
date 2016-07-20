﻿using LAN.Core.DependencyInjection;
using MongoDB.Driver;

namespace MonJobs.App
{ 
    public static class ServiceConfig
    {
        public static void Configure(IContainer container, IMongoDatabase database)
        {
            container.RegisterSingleton(database);
            container.Bind<IJobQueryService, MongoJobQueryService>(true);
            container.Bind<IJobCreationService, MongoJobCreationService>(true);
        }
    }
}