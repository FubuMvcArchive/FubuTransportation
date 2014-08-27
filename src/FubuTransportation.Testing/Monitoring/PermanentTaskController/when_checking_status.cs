using System;
using FubuTestingSupport;
using FubuTransportation.Monitoring;
using NUnit.Framework;

namespace FubuTransportation.Testing.Monitoring.PermanentTaskController
{
    [TestFixture]
    public class when_checking_status : PersistentTaskControllerContext
    {
        protected override void theContextIs()
        {
            Task("running://1").IsFullyFunctionalAndActive();
            Task("stopped://1").IsActive = false;
            Task("error://1").IsActiveButNotFunctional(new DivideByZeroException());
        }

        private HealthStatus findStatus(string uriString)
        {
            var uri = uriString.ToUri();
            var task = theController.CheckStatus(uri);
            task.Wait();

            return task.Result;
        }

        [Test]
        public void check_the_status_of_an_unknown_task()
        {
            findStatus("unknown://1").ShouldEqual(HealthStatus.Unknown);
        }

        [Test]
        public void check_the_status_of_an_active_and_functional_task()
        {
            findStatus("running://1").ShouldEqual(HealthStatus.Active);
        }

        [Test]
        public void check_the_status_of_an_inactive_task()
        {
            findStatus("stopped://1").ShouldEqual(HealthStatus.Inactive);
        }

        [Test]
        public void check_the_status_of_an_active_task_that_has_errored_out()
        {
            findStatus("error://1").ShouldEqual(HealthStatus.Error);
        }
    }
}