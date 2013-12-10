using System.Collections.Generic;
using FubuTransportation.Diagnostics;
using HtmlTags;
using Serenity;

namespace FubuTransportation.Serenity
{
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