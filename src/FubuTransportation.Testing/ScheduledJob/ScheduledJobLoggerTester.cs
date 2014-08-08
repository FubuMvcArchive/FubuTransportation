using System;
using System.Linq;
using System.Threading;
using FubuTestingSupport;
using FubuTransportation.Polling;
using FubuTransportation.ScheduledJob;
using NUnit.Framework;

namespace FubuTransportation.Testing.ScheduledJob
{
    [TestFixture]
    public class when_scheduled_job_succeeds : InteractionContext<ScheduledJobLogger>
    {
        private bool _executed;
        protected override void beforeEach()
        {
            _executed = false;
            RecordLogging();

            ClassUnderTest.LogAndTimeExecution(new ADummyJob(), () =>
            {
                Thread.Sleep(50); // 50 ms
                _executed = true;
            });
        }

        [Test]
        public void should_execute_the_action()
        {
            _executed.ShouldBeTrue();
        }

        [Test]
        public void should_log_job_start_success_finish()
        {
            var logRecords = RecordedLog().InfoMessages.ToList();
            logRecords.ShouldHaveCount(3);

            logRecords[0].ShouldBeOfType<ScheduledJobStarted>();
            logRecords[1].ShouldBeOfType<ScheduledJobSucceeded>();
            // Sometimes the stopwatch records a time that is less than the Thread.Sleep duration
            logRecords[2].ShouldBeOfType<ScheduledJobFinished>().Duration.ShouldBeGreaterThan(45); // 45 ms
        }
    }

    [TestFixture]
    public class when_scheduled_job_fails : InteractionContext<ScheduledJobLogger>
    {
        private bool _executed;
        private Exception _thrownException;

        protected override void beforeEach()
        {
            _executed = false;
            RecordLogging();

            try
            {
                ClassUnderTest.LogAndTimeExecution(new ADummyJob(), () =>
                {
                    Thread.Sleep(35); // 35 ms
                    _executed = true;

                    throw new ADummyTestException();
                });
            }
            catch (Exception ex)
            {
                _thrownException = ex;
            }
        }

        [Test]
        public void should_execute_the_action()
        {
            _executed.ShouldBeTrue();
        }

        [Test]
        public void should_log_job_start_failure_finish()
        {
            var logRecords = RecordedLog().InfoMessages.ToList();
            logRecords.ShouldHaveCount(3);

            logRecords[0].ShouldBeOfType<ScheduledJobStarted>();
            logRecords[1].ShouldBeOfType<ScheduledJobFailed>();
            // Sometimes the stopwatch records a time that is less than the Thread.Sleep duration
            logRecords[2].ShouldBeOfType<ScheduledJobFinished>().Duration.ShouldBeGreaterThan(30); // 30 ms
        }

        [Test]
        public void should_rethrow_exception()
        {
            _thrownException.ShouldBeOfType<ADummyTestException>();
        }
    }

    [TestFixture]
    public class when_job_is_scheduled : InteractionContext<ScheduledJobLogger>
    {
        protected DateTimeOffset _nextScheduledTime;

        protected override void beforeEach()
        {
            RecordLogging();
            _nextScheduledTime = DateTimeOffset.UtcNow;

            ClassUnderTest.LogNextScheduledRun(new ADummyJob(), _nextScheduledTime);
        }

        [Test]
        public void should_log_next_scheduled_time()
        {
            var logRecords = RecordedLog().InfoMessages.ToList();
            logRecords.ShouldHaveCount(1);

            logRecords[0].ShouldBeOfType<ScheduledJobScheduled>().ScheduledTime.ShouldEqual(_nextScheduledTime);
        }
    }

    public class ADummyJob : IJob
    {
        public void Execute()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Dummy Scheduled Job";
        }
    }

    public class ADummyTestException : Exception
    {}
}