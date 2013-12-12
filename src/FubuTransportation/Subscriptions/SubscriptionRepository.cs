using System;
using System.Collections.Generic;
using System.Linq;
using FubuTransportation.Configuration;

namespace FubuTransportation.Subscriptions
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ChannelGraph _graph;
        private readonly ISubscriptionPersistence _persistence;

        public SubscriptionRepository(ChannelGraph graph, ISubscriptionPersistence persistence)
        {
            _graph = graph;
            _persistence = persistence;
        }

        public void PersistSubscriptions(params Subscription[] requirements)
        {
            persist(requirements, SubscriptionRole.Subscribes);
        }

        private void persist(Subscription[] requirements, SubscriptionRole subscriptionRole)
        {
            requirements.Each(x => { x.Role = subscriptionRole; });

            var existing = _persistence.LoadSubscriptions(_graph.Name, subscriptionRole);
            var newReqs = requirements.Where(x => !existing.Contains(x)).ToArray();

            newReqs.Each(x => x.Id = Guid.NewGuid());
            _persistence.Persist(newReqs);
        }

        public void PersistPublishing(params Subscription[] subscriptions)
        {
            persist(subscriptions, SubscriptionRole.Publishes);
        }

        public IEnumerable<Subscription> LoadSubscriptions(SubscriptionRole role)
        {
            return _persistence.LoadSubscriptions(_graph.Name, role);
        }

        public IEnumerable<TransportNode> FindPeers()
        {
            return _persistence.NodesForGroup(_graph.Name);
        }

        public void SaveTransportNode()
        {
            var node = new TransportNode(_graph);
            if (!FindPeers().Contains(node))
            {
                node.Id = Guid.NewGuid();
                _persistence.Persist(node);
            }
        }
    }
}