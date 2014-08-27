using System.Collections.Generic;

namespace FubuTransportation.Subscriptions
{
    public interface ISubscriptionRepository
    {
        void PersistSubscriptions(params Subscription[] requirements);
        IEnumerable<Subscription> LoadSubscriptions(SubscriptionRole role);
        IEnumerable<TransportNode> FindPeers();
        void PersistPublishing(params Subscription[] subscriptions);

        void Persist(TransportNode node);
    }
}