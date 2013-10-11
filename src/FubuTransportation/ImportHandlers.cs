using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuTransportation.Configuration;
using FubuTransportation.ErrorHandling;

namespace FubuTransportation
{
    [ConfigurationType(ConfigurationType.Attachment)] // needs to be done AFTER authentication, so this is good
    public class ImportHandlers : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var handlers = graph.Settings.Get<HandlerGraph>();

            handlers.ApplyGeneralizedHandlers();

            var policies = graph.Settings.Get<HandlerPolicies>();
            handlers.ApplyPolicies(policies.GlobalPolicies);

            foreach (var chain in handlers)
            {
                // Apply the error handling node
                chain.InsertFirst(new ExceptionHandlerNode(chain));

                graph.AddChain(chain);
            }
        }
    }
}