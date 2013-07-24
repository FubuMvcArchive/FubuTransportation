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
            AddService<ITransport, RhinoQueuesTransport>().IsSingleton = true;
            SetServiceIfNone<IPersistentQueue, PersistentQueue>(x => x.IsSingleton = true);
        }
    }
}