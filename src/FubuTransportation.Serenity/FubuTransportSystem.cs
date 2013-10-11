using System.Collections.Generic;
using System.Linq;
using FubuCore.Binding;
using FubuMVC.Core;
using FubuTransportation.Configuration;
using FubuTransportation.Diagnostics;
using FubuTransportation.TestSupport;
using Serenity;
using StoryTeller.Engine;

namespace FubuTransportation.Serenity
{
    public class FubuTransportSystem<T> : FubuMvcSystem<T> where T : IApplicationSource, new()
    {
        public FubuTransportSystem()
        {
            FubuTransport.SetupForTesting();

            AddContextualProvider<MessageContextualInfoProvider>();
        }

        protected override void configureApplication(IApplicationUnderTest application, BindingRegistry binding)
        {
            var session = application.Services.GetInstance<IMessagingSession>();
            Bottles.Services.Messaging.EventAggregator.Messaging.AddListener(session);
        }

        public override IExecutionContext CreateContext()
        {
            IExecutionContext context = base.CreateContext();

            Application.Services.GetInstance<TransportCleanup>().ClearAll();

            SubSystems.OfType<RemoteSubSystem>().Each(x => x.Runner.SendRemotely(new ClearAllTransports()));

            return context;
        }
    }
}