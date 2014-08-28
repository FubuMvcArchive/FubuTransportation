using System.Collections.Generic;

namespace FubuTransportation.Subscriptions
{
    // TODO -- rename to ITransportNodeRepository
    public interface ISubscriptionRepository
    {
        void PersistSubscriptions(params Subscription[] requirements);
        IEnumerable<Subscription> LoadSubscriptions(SubscriptionRole role);
        IEnumerable<TransportNode> FindPeers();
        void PersistPublishing(params Subscription[] subscriptions);

        void Persist(params TransportNode[] nodes);

        TransportNode FindLocal();
    }
}