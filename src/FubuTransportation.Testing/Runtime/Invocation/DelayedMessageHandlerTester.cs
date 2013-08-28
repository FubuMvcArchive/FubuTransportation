using System;
using FubuCore.Dates;
using FubuMVC.Core.Runtime.Logging;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Delayed;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;
using FubuTestingSupport;
using Rhino.Mocks;
using System.Linq;

namespace FubuTransportation.Testing.Runtime.Invocation
{
    [TestFixture]
    public class DelayedMessageHandlerTester
    {
        [Test]
        public void matches_positive()
        {
            var systemTime = SystemTime.Default();
            var envelope = new Envelope();
            envelope.ExecutionTime = systemTime.UtcNow().AddHours(1);

            var handler = new DelayedMessageHandler(systemTime);

            envelope.IsDelayed(systemTime.UtcNow()).ShouldBeTrue();
            handler.Matches(envelope).ShouldBeTrue();
        }

        [Test]
        public void matches_negative_with_no_execution_time_header()
        {
            var systemTime = SystemTime.Default();
            var envelope = new Envelope();

            var handler = new DelayedMessageHandler(systemTime);

            envelope.IsDelayed(systemTime.UtcNow()).ShouldBeFalse();
            handler.Matches(envelope).ShouldBeFalse();
        }

        [Test]
        public void matches_negative_when_the_execution_time_is_in_the_past()
        {
            var systemTime = SystemTime.Default();
            var envelope = new Envelope();
            envelope.ExecutionTime = systemTime.UtcNow().AddHours(-1);

            var handler = new DelayedMessageHandler(systemTime);

            envelope.IsDelayed(systemTime.UtcNow()).ShouldBeFalse();
            handler.Matches(envelope).ShouldBeFalse();
        }

        [Test]
        public void execute_happy_path()
        {
            var logger = new RecordingLogger();
            var envelope = ObjectMother.Envelope();

            new DelayedMessageHandler(null).Execute(envelope, logger);

            envelope.Callback.AssertWasCalled(x => x.MoveToDelayed());

            logger.InfoMessages.Single().ShouldBeOfType<DelayedEnvelopeReceived>()
                  .Envelope.ShouldEqual(envelope.ToToken());
        }

        [Test]
        public void execute_sad_path()
        {
            var logger = new RecordingLogger();
            var envelope = ObjectMother.Envelope();

            var exception = new NotImplementedException();
            envelope.Callback.Stub(x => x.MoveToDelayed()).Throw(exception);

            new DelayedMessageHandler(SystemTime.Default()).Execute(envelope, logger);

            envelope.Callback.AssertWasCalled(x => x.MarkFailed());

            var report = logger.ErrorMessages.Single().ShouldBeOfType<ExceptionReport>();
            report.CorrelationId.ShouldEqual(envelope.CorrelationId);
            report.ExceptionText.ShouldEqual(exception.ToString());

        }


    }
}