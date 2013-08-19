using Bottles;
using FubuCore.Logging;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using FubuTransportation.Logging;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Runtime;

namespace FubuTransportation
{
    public class FubuTransportationExtensions : IFubuRegistryExtension
    {
        public void Configure(FubuRegistry registry)
        {
            // TODO -- need something that pulls HandlerGraph into the larger BehaviorGraph, just before instrumentation?
            // needs to be after authentication

            registry.Policies.Add<ImportHandlers>();
            registry.Services<FubuTransportServiceRegistry>();
        }
    }

    public class FubuTransportServiceRegistry : ServiceRegistry
    {
        public FubuTransportServiceRegistry()
        {
            // TODO -- this is awful.  Convenience method in 
            var eventAggregatorDef = ObjectDef.ForType<EventAggregator>();
            eventAggregatorDef.IsSingleton = true;
            SetServiceIfNone(typeof(IEventAggregator), eventAggregatorDef);

            var subscriberDef = ObjectDef.ForType<Subscriptions>();
            subscriberDef.IsSingleton = true;
            SetServiceIfNone(typeof (ISubscriptions), subscriberDef);

            SetServiceIfNone(typeof (ISagaStateCache<>), typeof (SagaStateCache<>));

            SetServiceIfNone<IMessageInvoker, MessageInvoker>();
            SetServiceIfNone<IEnvelopeSender, EnvelopeSender>();
            AddService<IMessageSerializer, XmlMessageSerializer>();
            AddService<IActivator, TransportActivator>();
            AddService<ITransport, InMemoryTransport>();

            SetServiceIfNone<IServiceBus, ServiceBus>();

            SetServiceIfNone<IEnvelopeSerializer, EnvelopeSerializer>();

            AddService<ILogListener, EventAggregationListener>();
        }
    }

    [ConfigurationType(ConfigurationType.Attachment)] // needs to be done AFTER authentication, so this is good
    public class ImportHandlers : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var handlers = graph.Settings.Get<HandlerGraph>();
            handlers.Add(HandlerCall.For<BatchHandler>(x => x.Handle(null)));

            handlers.ApplyGeneralizedHandlers();

            var policies = graph.Settings.Get<HandlerPolicies>();
            handlers.ApplyPolicies(policies.GlobalPolicies);

            foreach (var chain in handlers)
            {
                graph.AddChain(chain);
            }
        }
    }

}