using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Util;

namespace FubuTransportation.Subscriptions
{
    public class InMemorySubscriptionPersistence : ISubscriptionPersistence
    {
        private readonly Cache<Guid, Subscription> _subscriptions = new Cache<Guid, Subscription>();

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
    }
}