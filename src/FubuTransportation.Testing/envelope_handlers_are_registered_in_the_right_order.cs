using FubuMVC.Core;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;
using StructureMap;
using FubuMVC.StructureMap;
using FubuTestingSupport;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class envelope_handlers_are_registered_in_the_right_order
    {
        [Test]
        public void the_order()
        {
            var container = new Container();


            FubuApplication.DefaultPolicies().StructureMap(container).Bootstrap();

            var handlers = container.GetInstance<IHandlerPipeline>().ShouldBeOfType<HandlerPipeline>().Handlers;

            container.Dispose();
            handlers[0].ShouldBeOfType<DelayedEnvelopeHandler>();
            handlers[1].ShouldBeOfType<ResponseEnvelopeHandler>();
            handlers[2].ShouldBeOfType<ChainExecutionEnvelopeHandler>();
            handlers[3].ShouldBeOfType<NoSubscriberHandler>();
        }
    }
}