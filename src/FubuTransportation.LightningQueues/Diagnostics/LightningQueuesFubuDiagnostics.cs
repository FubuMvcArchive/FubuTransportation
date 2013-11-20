using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using FubuCore;
using HtmlTags;
using HtmlTags.Extended.TagBuilders;
using LightningQueues;

namespace FubuTransportation.LightningQueues.Diagnostics
{
    public class LightningQueuesFubuDiagnostics
    {
        private readonly IPersistentQueues _queues;

        public LightningQueuesFubuDiagnostics(IPersistentQueues queues)
        {
            _queues = queues;
        }

        public QueueManagersVisualization get_Index()
        {
            var visualization = new QueueManagersVisualization
            {
                QueueManagers = _queues.AllQueueManagers.Select(x => new QueueManagerModel(x)).ToList()
            };
            return visualization;
        }
    }

    public class QueueManagerModel
    {
        public QueueManagerModel(IQueueManager queueManager)
        {
            Queues = new QueueManagerTableTag(queueManager);
            EnableProcessedMessageHistory = queueManager.Configuration.EnableProcessedMessageHistory;
            EnableOutgoingMessageHistory = queueManager.Configuration.EnableOutgoingMessageHistory;
            Path = queueManager.Path;
            Port = queueManager.Endpoint.Port;
            OldestMessageInOutgoingHistory = queueManager.Configuration.OldestMessageInOutgoingHistory;
            OldestMessageInProcessedHistory = queueManager.Configuration.OldestMessageInProcessedHistory;
            NumberOfMessagesToKeepInOutgoingHistory = queueManager.Configuration.NumberOfMessagesToKeepInOutgoingHistory;
            NumberOfMessagesToKeepInProcessedHistory = queueManager.Configuration.NumberOfMessagesToKeepInProcessedHistory;
            NumberOfMessagIdsToKeep = queueManager.Configuration.NumberOfReceivedMessageIdsToKeep;
        }

        public int Port { get; set; }
        public string Path { get; set; }
        public bool EnableProcessedMessageHistory { get; set; }
        public bool EnableOutgoingMessageHistory { get; set; }
        public TimeSpan OldestMessageInOutgoingHistory { get; set; }
        public TimeSpan OldestMessageInProcessedHistory { get; set; }
        public int NumberOfMessagesToKeepInOutgoingHistory { get; set; }
        public int NumberOfMessagesToKeepInProcessedHistory { get; set; }
        public int NumberOfMessagIdsToKeep { get; set; }
        public QueueManagerTableTag Queues { get; set; }
    }

    public class QueueManagersVisualization
    {
        public IList<QueueManagerModel> QueueManagers { get; set; }
    }

    public class QueueManagerTableTag : TableTag
    {
        public QueueManagerTableTag(IQueueManager queueManager)
        {
            AddClass("table");

            AddHeaderRow(x =>
            {
                x.Header("Queue Name");
                x.Header("Message Count");
            });

            queueManager.Queues.Each(queueName =>
            {
                AddBodyRow(row => addQueueRow(row, queueManager, queueName));
                AddBodyRow(row => addQueueRow(row, queueManager, "{0}_history".ToFormat(queueName), "N/A"));
            });
            AddBodyRow(row => addQueueRow(row, queueManager, "outgoing", "N/A"));
            AddBodyRow(row => addQueueRow(row, queueManager, "outgoing_history", "N/A"));
        }

        private void addQueueRow(TableRowTag row, IQueueManager queueManager, string queueName, string displayForCount = null)
        {
            row.Cell().Add("a")
                .Attr("href", "messages/{0}/{1}"
                .ToFormat(queueManager.Endpoint.Port, queueName))
                .Text(queueName);
            
            row.Cell(displayForCount ?? queueManager.GetNumberOfMessages(queueName).ToString(CultureInfo.InvariantCulture));
        }
    }
}