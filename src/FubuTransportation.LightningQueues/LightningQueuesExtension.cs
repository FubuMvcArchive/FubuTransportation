using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuTransportation.Runtime;

namespace FubuTransportation.LightningQueues
{
    public class LightningQueuesExtension : IFubuRegistryExtension
    {
        public void Configure(FubuRegistry registry)
        {
            registry.Services<LightningQueuesServiceRegistry>();
        }
    }

    public class LightningQueuesServiceRegistry : ServiceRegistry
    {
        public LightningQueuesServiceRegistry()
        {
            AddService<ITransport, LightningQueuesTransport>();
            SetServiceIfNone<IPersistentQueues, PersistentQueues>(x => x.IsSingleton = true);
        }
    }
}