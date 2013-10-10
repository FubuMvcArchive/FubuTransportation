using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Util;

namespace FubuTransportation.Diagnostics
{
    public class MessagingSession : IMessagingSession
    {
        private readonly Cache<string, MessageHistory> _histories = new Cache<string, MessageHistory>(id => new MessageHistory{Id = id});

        public void ClearAll()
        {
            _histories.ClearAll();
        }

        public void Record(MessageRecord record)
        {
            if (record == null) return;

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
    }
}