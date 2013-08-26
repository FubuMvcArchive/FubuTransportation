using System;
using FubuTestingSupport;
using FubuTransportation.Polling;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Polling
{
    [TestFixture]
    public class when_running_a_job_successfully : InteractionContext<JobRunner<APollingJob>>
    {
        protected override void beforeEach()
        {
            ClassUnderTest.Run(new JobRequest<APollingJob>());
        }

        [Test]
        public void should_log_the_finish()
        {
            var theJob = MockFor<APollingJob>();
            MockFor<IPollingJobLogger>().AssertWasCalled(x => x.Successful(theJob));
        }

        [Test]
        public void should_log_the_start()
        {
            var theJob = MockFor<APollingJob>();
            MockFor<IPollingJobLogger>().AssertWasCalled(x => x.Starting(theJob));
        }

        [Test]
        public void should_run_the_internal_job()
        {
            MockFor<APollingJob>().AssertWasCalled(x => x.Execute());
        }
    }

    [TestFixture]
    public class when_running_a_job_with_a_job_failure : InteractionContext<JobRunner<APollingJob>>
    {
        private NotImplementedException theException;

        protected override void beforeEach()
        {
            theException = new NotImplementedException();
            MockFor<APollingJob>().Expect(x => x.Execute())
                                  .Throw(theException);

            ClassUnderTest.Run(new JobRequest<APollingJob>());
        }

        [Test]
        public void should_log_the_failure()
        {
            var theJob = MockFor<APollingJob>();
            MockFor<IPollingJobLogger>().AssertWasCalled(x => x.Failed(theJob, theException));
        }

        [Test]
        public void should_log_the_start()
        {
            var theJob = MockFor<APollingJob>();
            MockFor<IPollingJobLogger>().AssertWasCalled(x => x.Starting(theJob));
        }

        [Test]
        public void should_run_the_internal_job()
        {
            MockFor<APollingJob>().AssertWasCalled(x => x.Execute());
        }
    }

    public interface APollingJob : IJob
    {
    }
}