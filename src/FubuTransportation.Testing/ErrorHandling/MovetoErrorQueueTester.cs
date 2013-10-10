using System;
using FubuCore.Logging;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;
using Rhino.Mocks;
using FubuCore;
using FubuTestingSupport;

namespace FubuTransportation.Testing.ErrorHandling
{
    [TestFixture]
    public class when_the_move_to_error_queue_continuation_executes 
    {
        private Envelope theEnvelope;
        private NotImplementedException theException;
        private ILogger theLogger;
        private TestContinuationContext theContext;

        [SetUp]
        public void SetUp()
        {
            theEnvelope = ObjectMother.Envelope();
            theException = new NotImplementedException();

            theLogger = MockRepository.GenerateMock<ILogger>();

            theContext = new TestContinuationContext();

            new MoveToErrorQueue(theException).Execute(theEnvelope, theContext);
        }

        [Test]
        public void should_send_a_failure_acknowledgement()
        {
            theContext.RecordedOutgoing.FailureAcknowledgementMessage
                .ShouldEqual("Moved message {0} to the Error Queue.\n{1}".ToFormat(theEnvelope.CorrelationId, theException));
        }

        [Test]
        public void should_add_a_new_error_report()
        {
            var report = theEnvelope.Callback.GetArgumentsForCallsMadeOn(x => x.MoveToErrors(null))
                [0][0].As<ErrorReport>();

            report.ExceptionText.ShouldEqual(theException.ToString());
        }
    }
}