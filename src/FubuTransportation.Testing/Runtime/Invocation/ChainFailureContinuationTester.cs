using System;
using FubuMVC.Core.Runtime.Logging;
using FubuTransportation.Logging;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;
using Rhino.Mocks;
using System.Linq;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Runtime.Invocation
{
    [TestFixture]
    public class when_the_ChainFailureContinuation_Executes
    {
        private Exception theException;
        private RecordingLogger theLogger;
        private ChainFailureContinuation theContinuation;
        private Envelope theEnvelope;

        [SetUp]
        public void SetUp()
        {
            theException = new Exception();
            theLogger = new RecordingLogger();

            theContinuation = new ChainFailureContinuation(theException);

            theEnvelope = ObjectMother.Envelope();

            theContinuation.Execute(theEnvelope, theLogger);
        }

        [Test]
        public void should_mark_the_envelope_as_failed()
        {
            // TODO -- should this be going to the error or dead letter queue instead?
        
            theEnvelope.Callback.AssertWasCalled(x => x.MarkFailed());
        }

        [Test]
        public void should_log_the_message_failed()
        {
            theLogger.InfoMessages.Single().ShouldEqual(new MessageFailed
            {
                Envelope = theEnvelope.ToToken(),
                Exception = theException
            });
        }

        [Test]
        public void should_log_the_actual_exception()
        {
            var report = theLogger.ErrorMessages.Single()
                .ShouldBeOfType<ExceptionReport>();

            report.ExceptionText.ShouldEqual(theException.ToString());
        }
    }
}