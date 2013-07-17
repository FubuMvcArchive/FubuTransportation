using FubuMVC.Core.Registration.Nodes;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using NUnit.Framework;
using FubuCore;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class HandlerCallTester
    {
        // TODO -- need to do some end to end testing with these against the container

        [Test]
        public void choose_handler_type_for_one_in_one_out()
        {
            var handler = HandlerCall.For<ITargetHandler>(x => x.OneInOneOut(null));

            var objectDef = handler.As<IContainerModel>().ToObjectDef();

            objectDef.Type.ShouldEqual(typeof (CascadingHandlerInvoker<ITargetHandler, Input, Output>));
        }

        [Test]
        public void choose_handler_type_for_one_in_zero_out()
        {
            var handler = HandlerCall.For<ITargetHandler>(x => x.OneInZeroOut(null));

            var objectDef = handler.As<IContainerModel>().ToObjectDef();

            objectDef.Type.ShouldEqual(typeof(SimpleHandlerInvoker<ITargetHandler, Input>));
        }
    }
}