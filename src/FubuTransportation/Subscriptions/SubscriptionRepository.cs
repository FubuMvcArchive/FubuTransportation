using System;
using System.Collections.Generic;
using System.Linq;

namespace FubuTransportation.Subscriptions
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ISubscriptionPersistence _persistence;

        public SubscriptionRepository(ISubscriptionPersistence persistence)
        {
            _persistence = persistence;
        }

        public IEnumerable<Subscription> PersistRequirements(string name, params Subscription[] requirements)
        {
            var existing = _persistence.LoadSubscriptions(name);
            var newReqs = requirements.Where(x => !existing.Contains(x)).ToArray();

            newReqs.Each(x => x.Id = Guid.NewGuid());
            _persistence.Persist(newReqs);

            return existing.Union(newReqs).ToArray();
        }

        public IEnumerable<Subscription> LoadSubscriptions(string name)
        {
            return _persistence.LoadSubscriptions(name);
        }
    }
}