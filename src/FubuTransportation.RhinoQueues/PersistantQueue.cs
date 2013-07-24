using System;
using System.Linq;
using System.Net;
using Rhino.Queues;
using Rhino.Queues.Model;

namespace FubuTransportation.RhinoQueues
{
    public interface IPersistentQueue : IDisposable
    {
        IQueueManager QueueManager { get; }
        void Start();
        void Send(Uri destination, MessagePayload messagePayload);
        Message Receive(string queueName);
    }

    public class PersistentQueue : IPersistentQueue
    {
        private readonly RhinoQueuesSettings _settings;
        private readonly QueueManager _queueManager;

        public PersistentQueue(RhinoQueuesSettings settings)
        {
            _settings = settings;
            _queueManager = new QueueManager(new IPEndPoint(IPAddress.Loopback, settings.Port), "fubutransportation.esent");
        }

        public IQueueManager QueueManager { get { return _queueManager; } }

        public void Start()
        {
            var queueNames = _settings.Queues.Select(x => x.QueueName).ToArray();
            _queueManager.CreateQueues(queueNames);
            _queueManager.Start();
        }

        public void Send(Uri destination, MessagePayload messagePayload)
        {
            _queueManager.Send(destination, messagePayload);
        }

        public Message Receive(string queueName)
        {
            return _queueManager.Receive(queueName);
        }

        public void Dispose()
        {
            _queueManager.Dispose();
        }
    }
}