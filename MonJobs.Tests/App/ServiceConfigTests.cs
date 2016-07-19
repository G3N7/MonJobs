using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using LAN.Core.DependencyInjection.StructureMap;
using MonJobs.App;
using NUnit.Framework;
using StructureMap;

namespace MonJobs.Tests.App
{
    internal class ServiceConfigTests : MongoTestBase
    {
        [Test]
        public async Task RegisterDependencies_GivenAContainer_FillsInDependencyGraph()
        {
            var container = new StructureMapContainer(new Container());

            await RunInMongoLand(database =>
            {
                ServiceConfig.Configure(container, database);

                var bindingResult = container.AreAllRequiredDependenciesRegistered();

                Assert.That(bindingResult.IsMissingDependencies, Is.False, bindingResult.Message);
                return Task.FromResult(true);
            });

        }

        [Test]
        public async Task RegisterDependencies_HasAllControllersRegistered_SoWeGetValidationOfTheirDependencies()
        {
            var container = new StructureMapContainer(new Container());
            await RunInMongoLand(database =>
            {
                ServiceConfig.Configure(container, database);

                var bindingResult = container.AreAllRequiredDependenciesRegistered();

                if (bindingResult.IsMissingDependencies) Assert.Inconclusive();

                var assemblyWhereControllersLive = Assembly.GetAssembly(typeof(Startup));
                var allTypesInAssemblyWhereControllersLive = assemblyWhereControllersLive.GetTypes();
                var allControllers = allTypesInAssemblyWhereControllersLive
                    .Where(x => !x.IsAbstract)
                    .Where(x => typeof(ApiController).IsAssignableFrom(x));

                var controllers = allControllers as IList<Type> ?? allControllers.ToList();
                Assert.That(controllers, Is.Not.Empty);

                foreach (var controller in controllers)
                {
                    var controllerFromContainer = container.GetInstance(controller);
                    Assert.That(controllerFromContainer, Is.Not.Null, $"{controller.Name} Could not be created.");
                }
                return Task.FromResult(true);
            });
        }
    }
}
