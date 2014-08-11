using System;
using System.Linq;
using System.Threading.Tasks;
using FubuCore.Util;
using FubuTestingSupport;
using FubuTransportation.Polling;
using FubuTransportation.ScheduledJobs;
using NUnit.Framework;
using Rhino.Mocks;

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
        private StubJobExecutor theExecutor;
        private JobExecutionRecord theLastRun;

        [SetUp]
        public void SetUp()
        {
            theSchedule = new JobSchedule();


            theRule = new StubbedScheduleRule();

            theRule.ScheduledTimes[now] = next;

            theExecutor = new StubJobExecutor()
                .NowIs(now);

            theJob = new ScheduledJob<AJob>(theRule);

            theLastRun = new JobExecutionRecord();
            theSchedule.Find(theJob.JobType)
                .LastExecution = theLastRun;

            theJob.Initialize(theExecutor, theSchedule);
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


    }


    public class StubJobExecutor : IJobExecutor
    {
        private DateTimeOffset _now;


        public Task<JobExecutionRecord> Execute<T>() where T : IJob
        {
            throw new NotImplementedException();
        }

        public void ResetExecution<T>(IScheduledJob job, DateTimeOffset nextTime, JobExecutionRecord record)
        {
            throw new NotImplementedException();
        }

        public readonly Cache<Type, DateTimeOffset> Scheduled = new Cache<Type, DateTimeOffset>(); 

        public void Schedule<T>(IScheduledJob job, DateTimeOffset nextTime)
        {
            Scheduled[typeof (T)] = nextTime;
        }

        public StubJobExecutor NowIs(DateTimeOffset now)
        {
            _now = now;
            return this;
        }

        public DateTimeOffset Now()
        {
            return _now;
        }
    }
}