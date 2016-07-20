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
        }
    }
}
