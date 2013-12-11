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

        public IEnumerable<Subscription> PersistRequirements(params Subscription[] requirements)
        {
            var existing = _persistence.LoadSubscriptions(_graph.Name);
            var newReqs = requirements.Where(x => !existing.Contains(x)).ToArray();

            newReqs.Each(x => x.Id = Guid.NewGuid());
            _persistence.Persist(newReqs);

            return existing.Union(newReqs).ToArray();
        }

        public IEnumerable<Subscription> LoadSubscriptions()
        {
            return _persistence.LoadSubscriptions(_graph.Name);
        }
    }
}