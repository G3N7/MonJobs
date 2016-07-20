using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using LAN.Core.DependencyInjection;
using LAN.Core.DependencyInjection.StructureMap;
using Mongo2Go;
using MongoDB.Driver;
using MonJobs.Serialization;
using Newtonsoft.Json;
using StructureMap;

namespace MonJobs.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var structureMapContainer = new Container();
            var lanContainer = new StructureMapContainer(structureMapContainer);
            ContainerRegistry.DefaultContainer = lanContainer;

            var config = new HttpConfiguration
            {
                DependencyResolver = new StructureMapDependencyResolver(structureMapContainer)
            };

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
        }
    }
}
