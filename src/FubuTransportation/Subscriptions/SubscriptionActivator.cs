
using System;
using System.Collections.Generic;
using System.Linq;
using Bottles;
using Bottles.Diagnostics;
using FubuTransportation.Runtime;

namespace FubuTransportation.Subscriptions
{
    // Tested through Storyteller tests
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
            log.Trace("Determining subscriptions for node " + _cache.NodeName);

            _repository.SaveTransportNode();

            var requirements = determineStaticRequirements(log);


            if (requirements.Any())
            {
                log.Trace("Found static subscription requirements:");
                requirements.Each(x => log.Trace(x.ToString()));
            }
            else
            {
                log.Trace("No static subscriptions found from registry");
            }

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