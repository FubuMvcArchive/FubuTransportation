using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;

namespace FubuTransportation.Testing.Runtime.Invocation
{
    [TestFixture]
    public class WhenInvokingWithNoHandlerForMessageType : InteractionContext<HandlerPipeline>
    {
        private HandlerGraph theGraph;
 
        protected override void beforeEach()
        {
            theGraph = FubuTransportRegistry.HandlerGraphFor(x =>
            {
                x.Handlers.DisableDefaultHandlerSource();
                x.Handlers.Include<OneHandler>();
                x.Handlers.Include<TwoHandler>();
                x.Handlers.Include<ThreeHandler>();
                x.Handlers.Include<FourHandler>();
            });

            Services.Inject<HandlerGraph>(theGraph);
        }


        [Test]
        public void should_throw_the_no_handler_exception()
        {
            Exception<NoHandlerException>.ShouldBeThrownBy(() =>
            {
                var envelope = new Envelope { Message = new Events.Message1() };
                ClassUnderTest.Invoke(envelope); // we don't have a handler for this type
            })
            .Message.ShouldContain(typeof(Events.Message1).FullName);
        }
    }
}