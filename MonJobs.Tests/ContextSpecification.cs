using System.Threading.Tasks;
using NUnit.Framework;

namespace MonJobs.Tests
{
    abstract class ContextSpecification : TestBase
    {
        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            EstablishContext().GetAwaiter().GetResult();
            Because().GetAwaiter().GetResult();
        }
        
        protected virtual Task EstablishContext() { { return Task.FromResult(false); } }

        protected virtual Task Because() { { return Task.FromResult(false); } }

        [SetUp]
        public virtual Task BeforeEach() { return Task.FromResult(false); }

        [OneTimeTearDown]
        public virtual Task AfterAll() { return Task.FromResult(false); }

        [TearDown]
        public virtual Task AfterEach() { return Task.FromResult(false); }
    }
}
