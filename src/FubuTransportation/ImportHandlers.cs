using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuTransportation.Configuration;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Monitoring;
using FubuTransportation.Polling;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Subscriptions;

namespace FubuTransportation
{
    [ConfigurationType(ConfigurationType.Attachment)] // needs to be done AFTER authentication, so this is good
    public class ImportHandlers : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var handlers = graph.Settings.Get<HandlerGraph>();

            // TODO -- move this to a HandlerSource after we fix the duplicate calls
            // across HandlerSource problem.
            handlers.Add(HandlerCall.For<SubscriptionsHandler>(x => x.Handle(new SubscriptionRequested())));
            handlers.Add(HandlerCall.For<SubscriptionsHandler>(x => x.Handle(new SubscriptionsChanged())));
            handlers.Add(HandlerCall.For<SubscriptionsHandler>(x => x.Handle(new SubscriptionsRemoved())));


            handlers.Add(HandlerCall.For<MonitoringControlHandler>(x => x.Handle(new TakeOwnershipRequest())));
            handlers.Add(HandlerCall.For<MonitoringControlHandler>(x => x.Handle(new TaskHealthRequest())));
            handlers.Add(HandlerCall.For<MonitoringControlHandler>(x => x.Handle(new TaskDeactivation())));
            
            handlers.ApplyGeneralizedHandlers();

            var policies = graph.Settings.Get<HandlerPolicies>();
            handlers.ApplyPolicies(policies.GlobalPolicies);

            foreach (var chain in handlers)
            {
                // Apply the error handling node
                chain.InsertFirst(new ExceptionHandlerNode(chain));

                graph.AddChain(chain);

                // Hate how we're doing this, but disable tracing
                // on the polling job requests here.
                if (chain.InputType().Closes(typeof (JobRequest<>)))
                {
                    chain.Tags.Add(BehaviorChain.NoTracing);
                }
            }
        }
    }
}