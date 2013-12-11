using System.Collections.Generic;

namespace FubuTransportation.Subscriptions
{
    public interface ISubscriptionPersistence
    {
        IEnumerable<Subscription> LoadSubscriptions(string name);
        void Persist(IEnumerable<Subscription> subscriptions);
        void Persist(Subscription subscription);

        IEnumerable<TransportNode> NodesForGroup(string name);
        void Persist(TransportNode node);
    }
}