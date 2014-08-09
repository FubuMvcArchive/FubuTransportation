using System;
using System.Linq;
using FubuCore.Util;
using FubuTestingSupport;
using FubuTransportation.ScheduledJobs;
using NUnit.Framework;

namespace FubuTransportation.Testing.ScheduledJobs
{


    [TestFixture]
    public class when_rescheduling_a_job
    {
        private JobSchedule theSchedule;
        private StubbedScheduleRule theRule;
        private readonly DateTimeOffset now = DateTime.Today;
        private readonly DateTimeOffset next = DateTime.Today.AddHours(4);
        private ScheduledJob<AJob> theJob;

        [SetUp]
        public void SetUp()
        {
            theSchedule = new JobSchedule();

            theRule = new StubbedScheduleRule();

            theRule.ScheduledTimes[now] = next;

            theJob = new ScheduledJob<AJob>(theRule);
        }

        [Test]
        public void reschedule_if_the_next_time_is_null()
        {
            theJob.Reschedule(now, theSchedule);
            theSchedule.Find(theJob.JobType)
                .NextTime.ShouldEqual(next);

            theSchedule.Changes().Single().JobType.ShouldEqual(theJob.JobType);
        }

        [Test]
        public void reschedule_if_the_next_time_is_different()
        {
            theSchedule.Schedule(theJob.JobType, next.AddHours(-1));

            theJob.Reschedule(now, theSchedule);
            theSchedule.Find(theJob.JobType)
                .NextTime.ShouldEqual(next);

            theSchedule.Changes().Single().JobType.ShouldEqual(theJob.JobType);
        }

        [Test]
        public void no_change_do_nothing()
        {
            theSchedule = new JobSchedule(new[] {new JobStatus(typeof (AJob), next),});

            theJob.Reschedule(now, theSchedule);

            theSchedule.Find(theJob.JobType)
                .NextTime.ShouldEqual(next);

            theSchedule.Changes()
                .Any()
                .ShouldBeFalse();
        }
    }

    public class StubbedScheduleRule : IScheduleRule
    {
        public readonly Cache<DateTimeOffset, DateTimeOffset> ScheduledTimes
            = new Cache<DateTimeOffset, DateTimeOffset>();

        public DateTimeOffset ScheduleNextTime(DateTimeOffset currentTime)
        {
            return ScheduledTimes[currentTime];
        }
    }
}