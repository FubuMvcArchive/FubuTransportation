using System;
using FubuTestingSupport;
using FubuTransportation.Polling;
using FubuTransportation.ScheduledJob;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.ScheduledJob
{
    [TestFixture]
    public class when_running_a_scheduled_job_successfully : InteractionContext<ScheduledJobRunner<AScheduledJob>>
    {
        private IJob theJob;

        protected override void beforeEach()
        {
            theJob = MockFor<AScheduledJob>();
            MockFor<IScheduledJobLogger>()
                .Expect(x => x.LogAndTimeExecution(Arg<IJob>.Matches(j => ReferenceEquals(j, theJob)), Arg<Action>.Is.Anything))
                .WhenCalled(x =>
                {
                    var action = x.Arguments[1] as Action;
                    action();
                });

            ClassUnderTest.Execute(new ExecuteScheduledJob<AScheduledJob>());
        }

        [Test]
        public void should_run_the_internal_job()
        {
            MockFor<AScheduledJob>().AssertWasCalled(x => x.Execute());
        }
    }

    [TestFixture]
    public class when_running_a_scheduled_job_with_a_job_failure : InteractionContext<ScheduledJobRunner<AScheduledJob>>
    {
        private NotImplementedException theException;
        private Exception caughtException;
        private IJob theJob;

        protected override void beforeEach()
        {
            theException = new NotImplementedException();

            theJob = MockFor<AScheduledJob>();
            theJob.Expect(x => x.Execute()).Throw(theException);

            MockFor<IScheduledJobLogger>()
                .Expect(x => x.LogAndTimeExecution(Arg<IJob>.Matches(j => ReferenceEquals(j, theJob)), Arg<Action>.Is.Anything))
                .WhenCalled(x =>
                {
                    var action = x.Arguments[1] as Action;
                    action();
                });

            try
            {
                ClassUnderTest.Execute(new ExecuteScheduledJob<AScheduledJob>());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }
        }

        [Test]
        public void should_run_the_internal_job()
        {
            MockFor<AScheduledJob>().AssertWasCalled(x => x.Execute());
        }

        [Test]
        public void should_allow_exception_to_bubble_up_so_normal_message_retry_behavior_can_kick_in()
        {
            caughtException.ShouldNotBeNull();
            caughtException.ShouldBeOfType<NotImplementedException>();
        }
    }

    public interface AScheduledJob : IJob
    {
    }
}
