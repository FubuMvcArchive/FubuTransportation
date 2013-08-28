using System.Collections.Generic;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using Rhino.Mocks;
using Is = Rhino.Mocks.Constraints.Is;

namespace FubuTransportation.Testing.Runtime.Invocation
{
    [TestFixture]
    public class when_invoking_a_message_right_now_happy_path : InteractionContext<ChainInvoker>
    {
        private OneMessage theMessage;
        private HandlerGraph theGraph;
        private HandlerChain theExpectedChain;
        private StubServiceFactory theFactory;
        private object[] cascadingMessages;

        protected override void beforeEach()
        {
            theMessage = new OneMessage();
            theGraph = FubuTransportRegistry.HandlerGraphFor(x => {
                x.Handlers.Include<OneHandler>();
                x.Handlers.Include<TwoHandler>();
                x.Handlers.Include<ThreeHandler>();
                x.Handlers.Include<FourHandler>();
            });

            Services.Inject<HandlerGraph>(theGraph);

            theExpectedChain = theGraph.ChainFor(typeof (OneMessage));

            cascadingMessages = new object[] { new object(), new object(), new object() };
            theFactory = new StubServiceFactory(theExpectedChain, MockFor<IActionBehavior>(), cascadingMessages);
            Services.Inject<IServiceFactory>(theFactory);

            ClassUnderTest.InvokeNow(theMessage);

        }

        [Test]
        public void executed_the_proper_chain_for_the_input_type()
        {
            MockFor<IActionBehavior>().AssertWasCalled(x => x.Invoke());
        }

        [Test]
        public void cascaded_events_should_be_sent_to_the_bus()
        {
            cascadingMessages.Each(o => {

                // This ugly bit of code is just proving that we have indeed sent an envelope
                // where the inner message is one of our expected cascading messages
                MockFor<IEnvelopeSender>().AssertWasCalled(x => x.Send(null), x => x.Constraints(Is.Matching<Envelope>(e => e.Message == o)));
            });
        }
    }
}