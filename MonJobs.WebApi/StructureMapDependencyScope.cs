using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Dependencies;
using StructureMap;

namespace MonJobs.WebApi
{
    internal class StructureMapDependencyScope : IDependencyScope
    {
        private readonly IContainer _container;

        public StructureMapDependencyScope(IContainer container)
        {
            Contract.Requires(container != null);
            _container = container;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null) return null;

            if (serviceType.IsAbstract || serviceType.IsInterface)
            {
                return _container.TryGetInstance(serviceType);
            }

            return _container.GetInstance(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.GetAllInstances(serviceType).Cast<object>();
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}