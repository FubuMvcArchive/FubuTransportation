using System;
using FubuTestingSupport;
using FubuTransportation.ScheduledJob;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.ScheduledJob
{
    [TestFixture]
    public class when_initializing_a_scheduled_job : InteractionContext<ScheduledJobInitializer<AScheduledJob>>
    {
        private DateTimeOffset _scheduledSend;

        protected override void beforeEach()
        {
            LocalSystemTime = DateTime.UtcNow;
            _scheduledSend = LocalSystemTime.AddDays(1);

            MockFor<IJobScheduler>().Expect(x => x.ScheduleNextTime(Arg<DateTimeOffset>.Is.Equal((DateTimeOffset)UtcSystemTime)))
                .Return(_scheduledSend);

            ClassUnderTest.StartUp();
        }

        [Test]
        public void should_schedule_next_run_time_and_log_it()
        {
            MockFor<IScheduledJobLogger>().AssertWasCalled(
                x => x.LogNextScheduledRun(Arg<AScheduledJob>.Is.Anything, Arg<DateTimeOffset>.Is.Equal(_scheduledSend)));
        }

        [Test]
        public void should_send_delayed_message_to_queues_for_scheduling()
        {
            MockFor<IServiceBus>().AssertWasCalled(
                x => x.DelaySend(Arg<ExecuteScheduledJob<AScheduledJob>>.Is.Anything, Arg<DateTime>.Is.Equal(_scheduledSend.UtcDateTime)));
        }
    }
}