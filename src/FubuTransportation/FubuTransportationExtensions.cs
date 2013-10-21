using FubuCore.Descriptions;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuTransportation.Async;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using FubuTransportation.Polling;
using FubuTransportation.Runtime;
using FubuTransportation.Sagas;

namespace FubuTransportation
{
    public class FubuTransportationExtensions : IFubuRegistryExtension
    {
        public void Configure(FubuRegistry registry)
        {
            registry.Policies.Add<ImportHandlers>();
            registry.Services<FubuTransportServiceRegistry>();
            registry.Services<PollingServicesRegistry>();
            registry.Policies.Add<RegisterPollingJobs>();
            registry.Policies.Add<StatefulSagaConvention>();
            registry.Policies.Add<AsyncHandlingConvention>();

            if (FubuTransport.AllQueuesInMemory)
            {
                registry.Policies.Add<AllQueuesInMemoryPolicy>();
            }

            registry.Policies.Add<InMemoryQueueRegistration>();
        }
    }

    [System.ComponentModel.Description("Register the InMemoryTransport if enabled")]
    public class InMemoryQueueRegistration : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var enabled = FubuTransport.AllQueuesInMemory ||
                          graph.Settings.Get<TransportSettings>().EnableInMemoryTransport;

            if (enabled)
            {
                graph.Services.AddService<ITransport, InMemoryTransport>();
            }
        }
    }
}