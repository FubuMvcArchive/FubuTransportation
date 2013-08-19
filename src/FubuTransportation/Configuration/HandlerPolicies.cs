using System.ComponentModel;
using FubuCore.Descriptions;
using FubuMVC.Core.Configuration;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Diagnostics;

namespace FubuTransportation.Configuration
{
    [ApplicationLevel]
    public class HandlerPolicies
    {
        private readonly ConfigurationActionSet _globalPolicies = new ConfigurationActionSet("GlobalHandlers");

        public void AddGlobal(IConfigurationAction action, FubuTransportRegistry registry)
        {
            _globalPolicies.Fill(new Provenance[]{new FubuTransportRegistryProvenance(registry) }, action);
        }

        public ConfigurationActionSet GlobalPolicies
        {
            get { return _globalPolicies; }
        }
    }

    public class FubuTransportRegistryProvenance : Provenance
    {
        private readonly FubuTransportRegistry _registry;

        public FubuTransportRegistryProvenance(FubuTransportRegistry registry)
        {
            _registry = registry;
        }

        public override void Describe(Description description)
        {
            description.Title = "FubuTransportRegistry: " + _registry.GetType().Name;
        }
    }
}