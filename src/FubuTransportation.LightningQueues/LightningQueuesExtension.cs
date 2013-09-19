using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Delayed;
using LightningQueues.Model;

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
            SetServiceIfNone<IDelayedMessageCache<MessageId>, DelayedMessageCache<MessageId>>();
        }
    }
}