using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Util;
using FubuTransportation.Configuration;

namespace FubuTransportation.Diagnostics
{
    public class MessagingSession : IMessagingSession, Bottles.Services.Messaging.IListener<MessageRecord>
    {
        private readonly ChannelGraph _graph;
        private readonly Cache<string, MessageHistory> _histories = new Cache<string, MessageHistory>(id => new MessageHistory{Id = id});

        public MessagingSession(ChannelGraph graph)
        {
            _graph = graph;
        }

        public void ClearAll()
        {
            _histories.ClearAll();
        }

        public void Record(MessageRecord record)
        {
            if (record == null) return;

            record.Node = _graph.Name;

            // Letting the remote AppDomain's know about it.
            Bottles.Services.Messaging.EventAggregator.SendMessage(record);

            var history = _histories[record.Id];
            history.Record(record);

            if (record.ParentId != Guid.Empty.ToString())
            {
                var parent = _histories[record.ParentId];
                parent.AddChild(history); // this is idempotent, so we're all good
            }
        }

        public IEnumerable<MessageHistory> TopLevelMessages()
        {
            return _histories.Where(x => x.Parent == null);
        }

        public IEnumerable<MessageHistory> AllMessages()
        {
            return _histories;
        }

        public void Receive(MessageRecord message)
        {
            Record(message);
        }
    }
}