using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using NUnit.Framework;

namespace FubuTransportation.RhinoQueues.Testing
{
    //TODO test failures under resharper
    [TestFixture]
    public class RhinoQueuesServiceRegistry_spec
    {
        private void registeredTypeIs<TService, TImplementation>()
        {
            var registry = new FubuRegistry();
            registry.Services<RhinoQueuesServiceRegistry>();
            BehaviorGraph.BuildFrom(registry).Services.DefaultServiceFor<TService>().Type.ShouldEqual(
                typeof(TImplementation));
        }

        [Test]
        public void RhinoQueuesTransport_is_registered()
        {
            registeredTypeIs<ITransport, RhinoQueuesTransport>();
        }

        [Test]
        public void PersistentQueue_is_registered()
        {
            registeredTypeIs<IPersistentQueue, PersistentQueue>();
        }
    }
}