using Bottles;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuTransportation.Configuration;
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
            SetServiceIfNone<IMessageInvoker, MessageInvoker>();
            AddService<IMessageSerializer, XmlMessageSerializer>();
            SetServiceIfNone<IReceiver, Receiver>();
            AddService<IActivator, TransportActivator>();
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

            foreach (var chain in handlers)
            {
                graph.AddChain(chain);
            }
        }
    }

}