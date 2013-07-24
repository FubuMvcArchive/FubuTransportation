using System.Collections.Generic;

namespace FubuTransportation.RhinoQueues
{
    public class RhinoQueuesSettings
    {
        public RhinoQueuesSettings()
        {
            Queues = new List<QueueSetting>();
            Port = 2200;
        }

        public int Port { get; set; }
        public List<QueueSetting> Queues { get; set; }

        public RhinoQueuesSettings AddQueue(string queueName, int threadCount = 1)
        {
            Queues.Add(new QueueSetting{QueueName = queueName, ThreadCount = threadCount});
            return this;
        }
    }

    public class QueueSetting
    {
        public int ThreadCount { get; set; }
        public string QueueName { get; set; }
    }
}