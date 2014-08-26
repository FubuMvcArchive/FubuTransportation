using System;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Util;
using FubuTestingSupport;
using FubuTransportation.Polling;
using FubuTransportation.ScheduledJobs;
using NUnit.Framework;

namespace FubuTransportation.Testing.ScheduledJobs
{


    [TestFixture]
    public class when_initializing_a_job
    {
        private JobSchedule theSchedule;
        private StubbedScheduleRule theRule;
        private readonly DateTimeOffset now = DateTime.Today;
        private readonly DateTimeOffset next = DateTime.Today.AddHours(4);
        private ScheduledJob<AJob> theJob;
        private StubJobExecutor theExecutor;
        private JobExecutionRecord theLastRun;

        [SetUp]
        public void SetUp()
        {
            theSchedule = new JobSchedule();


            theRule = new StubbedScheduleRule();

            theRule.ScheduledTimes[now] = next;

            theExecutor = new StubJobExecutor();

            theJob = new ScheduledJob<AJob>(theRule);

            theLastRun = new JobExecutionRecord();
            theSchedule.Find(theJob.JobType)
                .LastExecution = theLastRun;

            theJob.As<IScheduledJob>().Initialize(now, theExecutor, theSchedule);
        }

        [Test]
        public void should_reset_the_new_job_status_time_for_record_keeping()
        {
            theSchedule.Find(theJob.JobType)
                .NextTime.ShouldEqual(next);
        }

        [Test]
        public void grabs_a_reference_to_the_last_execution_if_if_exists()
        {
            theJob.LastExecution.ShouldBeTheSameAs(theLastRun);
        }

        [Test]
        public void should_schedule_itself()
        {
            theExecutor.Scheduled[theJob.JobType]
                .ShouldEqual(next);
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


        public NextTimeExpression ReschedulesTo(DateTimeOffset nextTime)
        {
            return new NextTimeExpression(this, nextTime);
        }

        public class NextTimeExpression
        {
            private readonly StubbedScheduleRule _parent;
            private readonly DateTimeOffset _next;

            public NextTimeExpression(StubbedScheduleRule parent, DateTimeOffset next)
            {
                _parent = parent;
                _next = next;
            }

            public StubbedScheduleRule AtTime(DateTimeOffset now)
            {
                _parent.ScheduledTimes[now] = _next;
                return _parent;
            }
        }
    }


    public class StubJobExecutor : IJobExecutor
    {

        public void Execute<T>(TimeSpan timeout) where T : IJob
        {
            throw new NotImplementedException();
        }

        public void Reschedule<T>(IScheduledJob<T> job, DateTimeOffset nextTime, JobExecutionRecord record = null) where T : IJob
        {
            Scheduled[typeof(T)] = nextTime;
        }

        public void Schedule<T>(IScheduledJob<T> job, DateTimeOffset nextTime) where T : IJob
        {
            Scheduled[typeof(T)] = nextTime;
        }

        public readonly Cache<Type, DateTimeOffset> Scheduled = new Cache<Type, DateTimeOffset>();

    }
}