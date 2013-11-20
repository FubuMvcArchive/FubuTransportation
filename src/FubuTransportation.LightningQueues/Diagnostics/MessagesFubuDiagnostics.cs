using System.Collections.Generic;
using System.Linq;
using FubuCore;
using HtmlTags;
using LightningQueues.Model;

namespace FubuTransportation.LightningQueues.Diagnostics
{
    public class MessagesFubuDiagnostics
    {
        private readonly IPersistentQueues _queues;

        public MessagesFubuDiagnostics(IPersistentQueues queues)
        {
            _queues = queues;
        }

        public QueueMessagesVisualization get_messages_details_Port_QueueName(MessagesInputModel input)
        {
            var queueManager = _queues.AllQueueManagers.Single(x => x.Endpoint.Port == input.Port);

            var visualization = new QueueMessagesVisualization();
            if (input.QueueName == "outgoing")
            {
                visualization.Messages = new SendingMessagesTableTag(queueManager.GetMessagesCurrentlySending());
            }
            else if (input.QueueName == "outgoing_history")
            {
                visualization.Messages = new SendingMessagesTableTag(queueManager.GetAllSentMessages());
            }
            else if (input.QueueName.EndsWith("_history"))
            {
                visualization.Messages = new MessagesTableTag(queueManager.GetAllProcessedMessages(input.QueueName.Replace("_history", string.Empty)));
            }
            else
            {
                visualization.Messages = new MessagesTableTag(queueManager.GetAllMessages(input.QueueName, null));
            }
            return visualization;
        }
    }

    public class QueueMessagesVisualization
    {
        public string QueueName { get; set; }
        public TableTag Messages { get; set; }
    }

    public class MessagesInputModel
    {
        public int Port { get; set; }
        public string QueueName { get; set; }
    }

    public class MessagesTableTag : TableTag
    {
        public MessagesTableTag(IEnumerable<PersistentMessage> messages)
        {
            AddClass("table");

            AddHeaderRow(x =>
            {
                x.Header("Id");
                x.Header("Status");
                x.Header("Sent At");
                x.Header("Headers");
            });

            messages.Each(message => AddBodyRow(row => addMessageRow(row, message)));
        }

        private void addMessageRow(TableRowTag row, PersistentMessage message)
        {
            row.Cell(message.Id.ToString());
            row.Cell(message.Status.ToString());
            row.Cell(message.SentAt.ToString());
            var cell = row.Cell();
            var list = new HtmlTag("ul", cell);
            foreach (var key in message.Headers.AllKeys)
            {
                list.Add("li").Text("{0}&{1}".ToFormat(key, message.Headers[key]));
            }
        }
    }

    public class SendingMessagesTableTag : TableTag
    {
        public SendingMessagesTableTag(IEnumerable<PersistentMessageToSend> messages)
        {
            AddHeaderRow(x =>
            {
                x.Header("Id");
                x.Header("Status");
                x.Header("Sent At");
                x.Header("Destination");
                x.Header("Headers");
            });

            messages.Each(message => AddBodyRow(row => addMessageRow(row, message)));
        }

        private void addMessageRow(TableRowTag row, PersistentMessageToSend message)
        {
            row.Cell(message.Id.ToString());
            row.Cell(message.OutgoingStatus.ToString());
            row.Cell(message.SentAt.ToString());
            row.Cell(message.Endpoint.ToString());
            var cell = row.Cell();
            var list = new HtmlTag("ul", cell);
            foreach (var key in message.Headers.AllKeys)
            {
                list.Add("li").Text("{0}&{1}".ToFormat(key, message.Headers[key]));
            }
        }
    }
}