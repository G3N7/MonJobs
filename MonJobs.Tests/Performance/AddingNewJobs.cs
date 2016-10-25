using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MonJobs.Tests.Performance
{
    class AddingOneNewJobWithTenThousandOfEachExistingJob : TenThousandExistingOfEachTypeAcrossSixtyQueues
    {
        private DateTime StartTime;
        private DateTime EndTime;

        protected override async Task Because()
        {
            await base.Because().ConfigureAwait(false);
            StartTime = DateTime.Now;
            var creationService = new MongoJobCreationService(Database);
            await creationService.Create(GeneratedQueueId.First(), new JobAttributes()).ConfigureAwait(false);
            EndTime = DateTime.Now;
        }

        [Test]
        public void TakesLessThan5Ms()
        {
            var totalRuntime = EndTime.Subtract(StartTime).TotalMilliseconds;
            Console.WriteLine($"Total Runtime {totalRuntime} ms");
            Assert.That(totalRuntime, Is.LessThan(5));
        }
    }

    class AddingOneNewJobWithOneHundredThousandOfEachExistingJob : OneHundredThousandExistingOfEachTypeAcrossSixtyQueues
    {
        private DateTime StartTime;
        private DateTime EndTime;

        protected override async Task Because()
        {
            await base.Because().ConfigureAwait(false);
            StartTime = DateTime.Now;
            var creationService = new MongoJobCreationService(Database);
            await creationService.Create(GeneratedQueueId.First(), new JobAttributes()).ConfigureAwait(false);
            EndTime = DateTime.Now;
        }

        [Test]
        public void TakesLessThan5Ms()
        {
            var totalRuntime = EndTime.Subtract(StartTime).TotalMilliseconds;
            Console.WriteLine($"Total Runtime {totalRuntime} ms");
            Assert.That(totalRuntime, Is.LessThan(5));
        }
    }
}
