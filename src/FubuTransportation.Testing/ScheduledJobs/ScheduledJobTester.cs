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
    public class when_rescheduling_a_brand_new_job_that_completes_successfully
    {
        private StubJobExecutor theExecutor;
        private ScheduledJob<AJob> theJob;
        private JobExecutionRecord theRecord;

        [SetUp]
        public void SetUp()
        {
            theExecutor = new StubJobExecutor().NowIs(DateTime.Today);

            var rule = new StubbedScheduleRule()
                .ReschedulesTo(DateTime.Today.AddHours(1))
                .AtTime(theExecutor.Now());

            theJob = new ScheduledJob<AJob>(rule);

            theRecord = new JobExecutionRecord { Success = true };
            theJob.Reschedule(theRecord, theExecutor);
        }

        [Test]
        public void should_just_reschedule()
        {
            theExecutor.Scheduled[theJob.JobType]
                .ShouldEqual((DateTimeOffset)DateTime.Today.AddHours(1));
        }

        [Test]
        public void should_pass_along_the_new_record_as_is()
        {
            theExecutor.Recorded[theJob.JobType]
                .ShouldBeTheSameAs(theRecord);
        }

        [Test]
        public void should_track_the_last_execution()
        {
            theJob.LastExecution.ShouldBeTheSameAs(theRecord);
        }

    }

    [TestFixture]
    public class when_rescheduling_an_existing_job_that_completes_successfully
    {
        private StubJobExecutor theExecutor;
        private ScheduledJob<AJob> theJob;
        private JobExecutionRecord theRecord;

        [SetUp]
        public void SetUp()
        {
            theExecutor = new StubJobExecutor().NowIs(DateTime.Today);

            var rule = new StubbedScheduleRule()
                .ReschedulesTo(DateTime.Today.AddHours(1))
                .AtTime(theExecutor.Now());

            theJob = new ScheduledJob<AJob>(rule);
            theJob.LastExecution = new JobExecutionRecord();

            theRecord = new JobExecutionRecord { Success = true };
            theJob.Reschedule(theRecord, theExecutor);

        }

        [Test]
        public void should_just_reschedule()
        {
            theExecutor.Scheduled[theJob.JobType]
                .ShouldEqual((DateTimeOffset)DateTime.Today.AddHours(1));
        }

        [Test]
        public void should_pass_along_the_new_record_as_is()
        {
            theExecutor.Recorded[theJob.JobType]
                .ShouldBeTheSameAs(theRecord);
        }

        [Test]
        public void should_track_the_last_execution()
        {
            theJob.LastExecution.ShouldBeTheSameAs(theRecord);
        }

    }

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
        private DateTimeOffset _now;



        public Task<JobExecutionRecord> Execute<T>() where T : IJob
        {
            throw new NotImplementedException();
        }

        public void ResetExecution<T>(IScheduledJob job, DateTimeOffset nextTime, JobExecutionRecord record)
        {
            Scheduled[typeof(T)] = nextTime;
            Recorded[typeof (T)] = record;
        }

        public readonly Cache<Type, DateTimeOffset> Scheduled = new Cache<Type, DateTimeOffset>(); 
        public readonly Cache<Type, JobExecutionRecord> Recorded = new Cache<Type, JobExecutionRecord>(); 

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