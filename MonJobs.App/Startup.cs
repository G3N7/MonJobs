using System.Linq;
using System.Web.Http;
using LAN.Core.DependencyInjection;
using LAN.Core.DependencyInjection.StructureMap;
using Microsoft.Owin;
using MonJobs.Serialization;
using Newtonsoft.Json;
using Owin;
using StructureMap;

[assembly: OwinStartup(typeof(MonJobs.App.Startup))]

namespace MonJobs.App
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
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
            
            //ConfigureAuth(app);

            app.UseWebApi(config);
        }
    }
}
