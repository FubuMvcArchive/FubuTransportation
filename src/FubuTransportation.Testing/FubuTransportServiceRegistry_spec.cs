using Bottles;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using NUnit.Framework;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class FubuTransportServiceRegistry_spec
    {
        private void registeredTypeIs<TService, TImplementation>()
        {
            var registry = new FubuRegistry();
            registry.Services<FubuTransportServiceRegistry>();
            BehaviorGraph.BuildFrom(registry).Services.DefaultServiceFor<TService>().Type.ShouldEqual(
                typeof(TImplementation));
        }

        [Test]
        public void EnvelopeSerializer_is_registered()
        {
            registeredTypeIs<IEnvelopeSerializer, EnvelopeSerializer>();
        }

        [Test]
        public void MessageInvoker_is_registered()
        {
            registeredTypeIs<IMessageInvoker, MessageInvoker>();
        }

        [Test]
        public void XmlMessageSerializer_is_registered()
        {
            registeredTypeIs<IMessageSerializer, XmlMessageSerializer>();
        }

        [Test]
        public void TransportActivator_is_registered()
        {
            registeredTypeIs<IActivator, TransportActivator>();
        }

    }
}