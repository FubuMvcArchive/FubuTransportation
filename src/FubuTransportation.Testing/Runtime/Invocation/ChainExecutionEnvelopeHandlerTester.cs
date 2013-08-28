using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Runtime.Invocation
{
    [TestFixture]
    public class ChainExecutionEnvelopeHandlerTester : InteractionContext<ChainExecutionEnvelopeHandler>
    {
        private Envelope theEnvelope;
        private IMessageInvoker theInvoker;

        protected override void beforeEach()
        {
            theEnvelope = ObjectMother.Envelope();
            theInvoker = MockFor<IMessageInvoker>();
        }

        [Test]
        public void handle_with_no_matching_chain()
        {
            theInvoker.Stub(x => x.FindChain(theEnvelope))
                      .Return(null);

            ClassUnderTest.Handle(theEnvelope).ShouldBeNull();
        }

        [Test]
        public void handle_with_a_matching_chain()
        {
            var chain = new HandlerChain();

            theInvoker.Stub(x => x.FindChain(theEnvelope))
                      .Return(chain);

            var continuation = ClassUnderTest.Handle(theEnvelope).ShouldBeOfType<ChainExecution>();

            continuation.Chain.ShouldBeTheSameAs(chain);
            continuation.Invoker.ShouldBeTheSameAs(theInvoker);
        }
    }
}