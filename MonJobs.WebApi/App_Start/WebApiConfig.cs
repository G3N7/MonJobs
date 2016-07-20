using System;
using System.Web.Http;
using LAN.Core.DependencyInjection;
using LAN.Core.DependencyInjection.StructureMap;
using Mongo2Go;
using MongoDB.Driver;
using MonJobs.Serialization;
using Newtonsoft.Json;
using StructureMap;

namespace MonJobs.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var structureMapContainer = new Container();
            var lanContainer = new StructureMapContainer(structureMapContainer);
            ContainerRegistry.DefaultContainer = lanContainer;

            config.DependencyResolver = new StructureMapDependencyResolver(structureMapContainer);

            foreach (var typeConverter in MonJobsJsonConverters.TypesConverters)
            {
                config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(typeConverter);
            }

            JsonConvert.DefaultSettings = () => config.Formatters.JsonFormatter.SerializerSettings;
            MonJobsBsonConverters.RegisterConverters();

            var runner = MongoDbRunner.Start(System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N"));
            var server = new MongoClient(runner.ConnectionString);
            const string databaseName = "TEMP";
            var database = server.GetDatabase(databaseName);
            ServiceConfig.Configure(lanContainer, database);

            // Web API routes
            config.MapHttpAttributeRoutes();
            
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "queue/{queueId}/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
