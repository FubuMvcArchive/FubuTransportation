using System;
using System.Threading;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Util;
using FubuTestingSupport;
using FubuTransportation.Polling;
using FubuTransportation.ScheduledJobs;
using FubuTransportation.ScheduledJobs.Execution;
using FubuTransportation.ScheduledJobs.Persistence;
using NUnit.Framework;
using Rhino.Mocks;

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

    public abstract class ScheduledJobExecutionContext
    {
        protected readonly DateTimeOffset now = DateTime.Today;
        protected readonly DateTimeOffset theNextTimeAccordingToTheSchedulerRule = DateTime.Today.AddHours(4);
        protected readonly IJobRunTracker TheJobTracker = MockRepository.GenerateMock<IJobRunTracker>();
        protected Task<RescheduleRequest<RiggedJob>> theTask;
        protected readonly TimeSpan theConfiguredTimeout = 1.Seconds();

        [TestFixtureSetUp]
        public void SetUp()
        {
            var rule = new StubbedScheduleRule();
            rule.ScheduledTimes[now] = theNextTimeAccordingToTheSchedulerRule;

            TheJobTracker.Stub(x => x.Now()).Return(now);

            var job = theJobIs();

            var scheduledJob = new ScheduledJob<RiggedJob>(rule);
            scheduledJob.Timeout = theConfiguredTimeout;
            theTask = scheduledJob.ToTask(job, TheJobTracker);

            try
            {
                theTask.Wait();
            }
            catch (Exception)
            {
                // okay to swallow because you'll
                // check it on task itself
            }
        }

        protected abstract RiggedJob theJobIs();
    }

    [TestFixture]
    public class when_running_a_job_happy_path : ScheduledJobExecutionContext
    {
        protected override RiggedJob theJobIs()
        {
            return new RiggedJob
            {
                Exception = null,
                TimeToRun = 0.Minutes()
            };
        }

        [Test]
        public void should_mark_the_job_as_executing_correctly_and_reschedules()
        {
            TheJobTracker.AssertWasCalled(x => x.Success(theNextTimeAccordingToTheSchedulerRule));
        }

        [Test]
        public void should_return_the_reschedule_request_message()
        {
            theTask.Result.NextTime.ShouldEqual(theNextTimeAccordingToTheSchedulerRule);
        }

        [Test]
        public void should_run_the_to_completion()
        {
            theTask.IsCompleted.ShouldBeTrue();
        }
    }

    [TestFixture]
    public class when_running_a_job_that_times_out : ScheduledJobExecutionContext
    {
        protected override RiggedJob theJobIs()
        {
            return new RiggedJob
            {
                Exception = null,
                TimeToRun = theConfiguredTimeout.Add(5.Seconds())
            };
        }

        [Test]
        public void should_fault_with_a_timeout()
        {
            theTask.IsFaulted.ShouldBeTrue();
            theTask.Exception.Flatten().InnerException.ShouldBeOfType<TimeoutException>();
        }

        [Test]
        public void should_track_the_job_failure()
        {
            var exception = TheJobTracker.GetArgumentsForCallsMadeOn(x => x.Failure(null))
                [0][0].ShouldBeOfType<AggregateException>();

            exception.Flatten().InnerException.ShouldBeOfType<TimeoutException>();
        }
    }

    [TestFixture]
    public class when_running_a_job_that_fails_before_timing_out : ScheduledJobExecutionContext
    {
        protected override RiggedJob theJobIs()
        {
            return new RiggedJob
            {
                Exception = new DivideByZeroException()
            };
        }

        [Test]
        public void should_fault_with_the_job_exception()
        {
            theTask.IsFaulted.ShouldBeTrue();
            theTask.Exception.Flatten().InnerException.ShouldBeOfType<DivideByZeroException>();
        }

        [Test]
        public void should_track_the_job_failure()
        {
            var exception = TheJobTracker.GetArgumentsForCallsMadeOn(x => x.Failure(null))
                [0][0].ShouldBeOfType<AggregateException>();

            exception.Flatten().InnerException.ShouldBeOfType<DivideByZeroException>();
        }
    }



    public class RiggedJob : IJob
    {
        public bool WasCancelled;
        public bool Finished;

        public Exception Exception = null;
        public TimeSpan TimeToRun = 0.Minutes();


        public void Execute(CancellationToken cancellation)
        {
            if (Exception != null) throw Exception;

            var ending = DateTime.UtcNow.Add(TimeToRun);
            while (DateTime.UtcNow <= ending)
            {
                Thread.Sleep(100);
                if (cancellation.IsCancellationRequested)
                {
                    WasCancelled = true;
                    cancellation.ThrowIfCancellationRequested();
                }
            }

            Finished = true;
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