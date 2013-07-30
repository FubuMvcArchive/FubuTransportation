using System.Net;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuTransportation.Runtime;
using Rhino.Queues;

namespace FubuTransportation.RhinoQueues
{
    public class RhinoQueuesExtension : IFubuRegistryExtension
    {
        public void Configure(FubuRegistry registry)
        {
            registry.Services<RhinoQueuesServiceRegistry>();
        }
    }

    public class RhinoQueuesServiceRegistry : ServiceRegistry
    {
        public RhinoQueuesServiceRegistry()
        {
            AddService<ITransport, RhinoQueuesTransport>();
            SetServiceIfNone<IPersistentQueues, PersistentQueues>(x => x.IsSingleton = true);
        }
    }
}