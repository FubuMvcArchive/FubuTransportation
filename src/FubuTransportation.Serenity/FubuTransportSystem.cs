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
            FubuTransport.SetupForTesting(); // Uses FubuMode.SetUpTestingMode();

            AddContextualProvider<MessageContextualInfoProvider>();

            OnStartup<IMessagingSession>(x => {
                Bottles.Services.Messaging.EventAggregator.Messaging.AddListener(x);
                RemoteSubSystems.Each(sys => sys.Runner.Messaging.AddListener(x));
            });

            OnContextCreation<TransportCleanup>(cleanup => {
                cleanup.ClearAll();

                RemoteSubSystems.Each(x => x.Runner.SendRemotely(new ClearAllTransports()));
            });
        }
    }
}