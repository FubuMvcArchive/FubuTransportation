using FubuMVC.Core.Runtime.Logging;
using FubuTestingSupport;
using FubuTransportation.Logging;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;
using Rhino.Mocks;
using System.Linq;
using System.Collections.Generic;

namespace FubuTransportation.Testing.Runtime.Invocation
{
    [TestFixture]
    public class ChainSuccessContinuationTester
    {
        private Envelope theEnvelope;
        private FubuTransportation.Runtime.Invocation.InvocationContext theContext;
        private RecordingEnvelopeSender theSender;
        private ChainSuccessContinuation theContinuation;
        private RecordingLogger theLogger;

        [SetUp]
        public void SetUp()
        {
            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Message = new object();

            theContext = new FubuTransportation.Runtime.Invocation.InvocationContext(theEnvelope);

            theContext.EnqueueCascading(new object());
            theContext.EnqueueCascading(new object());
            theContext.EnqueueCascading(new object());

            theSender = new RecordingEnvelopeSender();

            theContinuation = new ChainSuccessContinuation(theSender, theContext);

            theLogger = new RecordingLogger();

            theContinuation.Execute(theEnvelope, theLogger);
        }

        [Test]
        public void should_mark_the_message_as_successful()
        {
            theEnvelope.Callback.AssertWasCalled(x => x.MarkSuccessful());
        }

        [Test]
        public void should_log_the_chain_success()
        {
            theLogger.InfoMessages.Single()
                     .ShouldEqual(new MessageSuccessful {Envelope = theEnvelope.ToToken()});
        }

        [Test]
        public void all_the_cascading_messages_are_sent()
        {
            theContext.OutgoingMessages().Each(o => {
                theSender.Sent.Single(x => x.Message == o)
                         .ParentId.ShouldEqual(theEnvelope.CorrelationId);
            });
        }

    }
}