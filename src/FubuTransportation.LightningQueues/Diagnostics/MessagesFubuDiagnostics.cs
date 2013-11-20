using System.Collections.Generic;
using System.Linq;
using HtmlTags;
using LightningQueues.Model;
using LightningQueues.Protocol;

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

            PersistentMessage[] messages;
            if (input.QueueName == "outgoing")
            {
                messages = queueManager.GetMessagesCurrentlySending();
            }
            else if (input.QueueName == "outgoing_history")
            {
                messages = queueManager.GetAllSentMessages();
            }
            else if (input.QueueName.EndsWith("_history"))
            {
                messages = queueManager.GetAllProcessedMessages(input.QueueName.Replace("_history", string.Empty));
            }
            else
            {
                messages = queueManager.GetAllMessages(input.QueueName, null);
            }
            return new QueueMessagesVisualization {Messages = new MessagesTableTag(messages), QueueName = input.QueueName};
        }
    }

    public class QueueMessagesVisualization
    {
        public string QueueName { get; set; }
        public MessagesTableTag Messages { get; set; }
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
            row.Cell(message.Headers.ToQueryString());
        }
    }
}