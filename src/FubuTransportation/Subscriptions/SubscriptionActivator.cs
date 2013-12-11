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
        private readonly ChannelGraph _graph;
        private readonly IEnumerable<ISubscriptionRequirement> _requirements;

        public SubscriptionActivator(ChannelGraph graph, ISubscriptionRepository repository, IEnvelopeSender sender, ISubscriptionCache cache, IEnumerable<ISubscriptionRequirement> requirements)
        {
            _graph = graph;
            _repository = repository;
            _sender = sender;
            _cache = cache;
            _requirements = requirements;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            _repository.SaveTransportNode();

            var requirements = _requirements.SelectMany(x => x.DetermineRequirements()).ToArray();

            log.Trace("Found subscription requirements:");
            requirements.Each(x => log.Trace(x.ToString()));

            var subscriptions = _repository.PersistRequirements(requirements).ToArray();
            

            subscriptions.GroupBy(x => x.Source).Each(group => {
                var envelope = new Envelope
                {
                    Message = new SubscriptionRequested
                    {
                        Subscriptions = @group.ToArray()
                    },
                    Destination = @group.Key
                };

                _sender.Send(envelope);
            });

            _cache.LoadSubscriptions(subscriptions);
        }
    }
}