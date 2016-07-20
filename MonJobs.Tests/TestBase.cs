using System.Linq;
using MonJobs.Serialization;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MonJobs.Tests
{
    internal abstract class TestBase
    {
        private static bool _hasRegistered;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            if (!_hasRegistered)
            {
                _hasRegistered = true;
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    Converters = MonJobsJsonConverters.TypesConverters.ToList()
                };
                MonJobsBsonConverters.RegisterConverters();
            }
        }
    }
}