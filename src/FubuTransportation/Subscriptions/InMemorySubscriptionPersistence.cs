using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuCore.Util;

namespace FubuTransportation.Subscriptions
{
    public class InMemorySubscriptionPersistence : ISubscriptionPersistence
    {
        private readonly Cache<Guid, Subscription> _subscriptions = new Cache<Guid, Subscription>();
        private readonly IList<TransportNode> _nodes = new List<TransportNode>(); 

        public IEnumerable<Subscription> LoadSubscriptions(string name)
        {
            return _subscriptions.Where(x => FubuCore.StringExtensions.EqualsIgnoreCase(x.NodeName, name)).ToArray();
        }

        public void Persist(IEnumerable<Subscription> subscriptions)
        {
            subscriptions.Each(Persist);
        }

        public void Persist(Subscription subscription)
        {
            if (subscription.Id == Guid.Empty)
            {
                subscription.Id = Guid.NewGuid();
            }

            _subscriptions[subscription.Id] = subscription;
        }

        public IEnumerable<TransportNode> NodesForGroup(string name)
        {
            return _nodes.Where(x => x.NodeName.EqualsIgnoreCase(name));
        }

        public void Persist(TransportNode node)
        {
            if (node.Id == Guid.Empty)
            {
                node.Id = Guid.NewGuid();
            }

            _nodes.Fill(node);
        }
    }
}