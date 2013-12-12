using System;
using System.Collections.Generic;
using System.Linq;
using Bottles;
using Bottles.Diagnostics;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;

namespace FubuTransportation.Subscriptions
{
    public class SubscriptionActivator : IActivator
    {
        private readonly ISubscriptionRepository _repository;
        private readonly IEnvelopeSender _sender;
        private readonly ISubscriptionCache _cache;
        private readonly IEnumerable<ISubscriptionRequirement> _requirements;

        public SubscriptionActivator(ISubscriptionRepository repository, IEnvelopeSender sender, ISubscriptionCache cache, IEnumerable<ISubscriptionRequirement> requirements)
        {
            _repository = repository;
            _sender = sender;
            _cache = cache;
            _requirements = requirements;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            _repository.SaveTransportNode();

            var requirements = determineStaticRequirements(log);

            _repository.PersistSubscriptions(requirements);

            var subscriptions = _repository.LoadSubscriptions(SubscriptionRole.Subscribes);
            _cache.LoadSubscriptions(subscriptions);

            sendSubscriptions(subscriptions);
        }

        private Subscription[] determineStaticRequirements(IPackageLog log)
        {
            var requirements = _requirements.SelectMany(x => x.DetermineRequirements()).ToArray();
            traceLoadedRequirements(log, requirements);
            return requirements;
        }

        private void sendSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            subscriptions
                .GroupBy(x => x.Source)
                .Each(group => sendSubscriptionsToSource(@group.Key, @group));
        }

        private static void traceLoadedRequirements(IPackageLog log, Subscription[] requirements)
        {
            log.Trace("Found subscription requirements:");
            requirements.Each(x => log.Trace(x.ToString()));
        }

        private void sendSubscriptionsToSource(Uri destination, IEnumerable<Subscription> subscriptions)
        {
            var envelope = new Envelope
            {
                Message = new SubscriptionRequested
                {
                    Subscriptions = subscriptions.ToArray()
                },
                Destination = destination
            };

            _sender.Send(envelope);
        }
    }
}