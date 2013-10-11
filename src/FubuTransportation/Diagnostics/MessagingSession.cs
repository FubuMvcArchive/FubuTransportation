using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FubuCore.Util;
using FubuTransportation.Configuration;

namespace FubuTransportation.Diagnostics
{
    public class MessagingSession : IMessagingSession, Bottles.Services.Messaging.IListener<MessageRecord>
    {
        private readonly ChannelGraph _graph;
        private readonly Cache<string, MessageHistory> _histories = new Cache<string, MessageHistory>(id => new MessageHistory{Id = id});
        private readonly IList<MessageRecord> _all = new List<MessageRecord>(); 

        public MessagingSession(ChannelGraph graph)
        {
            _graph = graph;
        }

        public void ClearAll()
        {
            _histories.ClearAll();
            _all.Clear();
        }

        public void Record(MessageRecord record)
        {
            if (record == null) return;

            if (_all.Contains(record)) return;
            _all.Add(record);

            Debug.WriteLine("Got MessageRecord: " + record);

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