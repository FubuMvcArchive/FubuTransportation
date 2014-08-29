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
            requirements.Each(x => {
                x.NodeName = _graph.Name;
                x.Role = subscriptionRole;
            });

            var existing = _persistence.LoadSubscriptions(_graph.Name, subscriptionRole);
            var newReqs = requirements.Where(x => !existing.Contains(x)).ToArray();

            newReqs.Each(x => x.Id = Guid.NewGuid());
            _persistence.Persist(newReqs);
        }

        public void PersistPublishing(params Subscription[] subscriptions)
        {
            persist(subscriptions, SubscriptionRole.Publishes);
        }

        public void Persist(params TransportNode[] nodes)
        {
            _persistence.Persist(nodes);
        }

        public TransportNode FindLocal()
        {
            return _persistence.LoadNode(_graph.NodeId);
        }

        public TransportNode FindPeer(string nodeId)
        {
            return _persistence.LoadNode(nodeId);
        }

        public void AddOwnershipToThisNode(Uri subject)
        {
            _persistence.Alter(_graph.NodeId, node => node.AddOwnership(subject));
        }

        public void AddOwnershipToThisNode(IEnumerable<Uri> subjects)
        {
            _persistence.Alter(_graph.NodeId, node => node.AddOwnership(subjects));
        }

        public void RemoveOwnershipFromThisNode(Uri subject)
        {
            _persistence.Alter(_graph.NodeId, node => node.RemoveOwnership(subject));
        }

        public void RemoveOwnershipFromNode(string nodeId, Uri subject)
        {
            _persistence.Alter(nodeId, node => node.RemoveOwnership(subject));
        }

        public IEnumerable<Subscription> LoadSubscriptions(SubscriptionRole role)
        {
            return _persistence.LoadSubscriptions(_graph.Name, role);
        }

        public IEnumerable<TransportNode> FindPeers()
        {
            return _persistence.NodesForGroup(_graph.Name);
        }
    }
}