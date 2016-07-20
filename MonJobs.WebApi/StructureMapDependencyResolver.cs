using System.Web.Http.Dependencies;
using StructureMap;

namespace MonJobs.WebApi
{
    internal class StructureMapDependencyResolver : StructureMapDependencyScope, IDependencyResolver
    {
        private readonly IContainer _container;

        public StructureMapDependencyResolver(IContainer container) : base(container)
        {
            _container = container;
        }

        public IDependencyScope BeginScope()
        {
            var childContainer = _container.GetNestedContainer();
            return new StructureMapDependencyScope(childContainer);
        }
    }
}