using System.Collections.Generic;
using FubuCore.Binding;
using FubuMVC.Core;
using FubuTransportation.Configuration;
using FubuTransportation.Diagnostics;
using HtmlTags;
using Serenity;

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
    }

    public class MessageContextualInfoProvider : IContextualInfoProvider
    {
        private readonly IMessagingSession _session;

        public MessageContextualInfoProvider(IMessagingSession session)
        {
            _session = session;
        }

        public void Reset()
        {
            _session.ClearAll();
        }

        public IEnumerable<HtmlTag> GenerateReports()
        {
            yield return new HtmlTag("h3").Text("Message History");

            foreach (MessageHistory topLevelMessage in _session.TopLevelMessages())
            {
                yield return topLevelMessage.ToNodeTag();
            }

            foreach (MessageHistory history in _session.AllMessages())
            {
                yield return new MessageHistoryTableTag(history);
            }
        }
    }
}