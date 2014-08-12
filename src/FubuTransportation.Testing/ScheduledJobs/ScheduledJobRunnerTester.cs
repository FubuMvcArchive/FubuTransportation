using System;
using System.Threading;
using FubuCore;
using FubuCore.Dates;
using FubuCore.Logging;
using FubuTestingSupport;
using FubuTransportation.Polling;
using FubuTransportation.ScheduledJobs;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.ScheduledJobs
{
    public abstract class ScheduledJobRunnerContext
    {
        protected RecordingLogger theLogger;
        protected ISettableClock theClock;
        protected JobExecutionRecord theRecord;
        protected IScheduleRepository TheRepository;

        [TestFixtureSetUp]
        public void SetUp()
        {
            theLogger = new RecordingLogger();
            theClock = new SettableClock().LocalNow(DateTime.Today.AddHours(8));

            var job = new AScheduledJob();

            TheRepository = MockRepository.GenerateMock<IScheduleRepository>();

            theJobRunsLike(job);

            theRecord = new ScheduledJobRunner<AScheduledJob>(job, theLogger, theClock, TheRepository)
                .Execute(new ExecuteScheduledJob<AScheduledJob>());
        }


        protected abstract void theJobRunsLike(AScheduledJob job);
    }

    [TestFixture]
    public class when_running_a_scheduled_job_successfully : ScheduledJobRunnerContext
    {
        protected override void theJobRunsLike(AScheduledJob job)
        {
            job.Duration = 100;
            job.Exception = null;
        }

        [Test]
        public void should_mark_the_job_as_executing()
        {
            TheRepository.AssertWasCalled(x => x.MarkExecuting<AScheduledJob>());
        }

        [Test]
        public void should_later_mark_the_job_as_completed()
        {
            TheRepository.AssertWasCalled(x => x.MarkCompletion<AScheduledJob>(theRecord));
        }

        [Test]
        public void should_record_the_duration_of_the_job()
        {
            theRecord.Duration.ShouldBeGreaterThan(75);
        }

        [Test]
        public void records_success()
        {
            theRecord.Success.ShouldBeTrue();
        }

        [Test]
        public void exception_text_should_be_empty()
        {
            theRecord.ExceptionText.IsEmpty().ShouldBeTrue();
        }
    }

    [TestFixture]
    public class when_running_a_scheduled_job_with_a_job_failure : ScheduledJobRunnerContext
    {
        private readonly Exception EX = new Exception("You stink!");

        protected override void theJobRunsLike(AScheduledJob job)
        {
            job.Duration = 100;
            job.Exception = EX;
        }

        [Test]
        public void should_mark_the_job_as_executing()
        {
            TheRepository.AssertWasCalled(x => x.MarkExecuting<AScheduledJob>());
        }

        [Test]
        public void should_later_mark_the_job_as_completed()
        {
            TheRepository.AssertWasCalled(x => x.MarkCompletion<AScheduledJob>(theRecord));
        }


        [Test]
        public void should_record_the_duration_of_the_job()
        {
            theRecord.Duration.ShouldBeGreaterThan(75);
        }

        [Test]
        public void records_a_failure()
        {
            theRecord.Success.ShouldBeFalse();
        }

        [Test]
        public void exception_text_should_be_empty()
        {
            theRecord.ExceptionText.ShouldEqual(EX.ToString());
        }
    }

    public class AScheduledJob : IJob
    {
        public Exception Exception = null;
        public int Duration = 100;

        public void Execute()
        {
            Thread.Sleep(Duration);

            if (Exception != null)
            {
                throw Exception;
            }
        }
    }
}