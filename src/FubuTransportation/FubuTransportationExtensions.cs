﻿using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuTransportation.Async;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using FubuTransportation.Polling;
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
        }
    }

    public class FubuTransportTestingModeServiceRegistry : ServiceRegistry
    {
        public FubuTransportTestingModeServiceRegistry()
        {
        }
    }
}