using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FubuCore.Util;
using FubuTransportation.Runtime;
using Rhino.Queues;

namespace FubuTransportation.RhinoQueues
{
    public class PersistentQueues : IPersistentQueues
    {
        // TODO -- Corey, does this need to be unique per endpoint??????
        public const string EsentPath = "fubutransportation.esent";

        private readonly Cache<IPEndPoint, QueueManager> _queueManagers =
            new Cache<IPEndPoint, QueueManager>(ip => new QueueManager(ip, EsentPath));


//        public void Start()
//        {
//            var queueNames = _settings.Queues.Select(x => x.QueueName).ToArray();
//            _queueManager.CreateQueues(queueNames);
//            _queueManager.Start();
//        }
//
//        public void Send(Uri destination, MessagePayload messagePayload)
//        {
//            _queueManager.Send(destination, messagePayload);
//        }
//
//        public Message Receive(string queueName)
//        {
//            return _queueManager.Receive(queueName);
//        }

        public void Dispose()
        {
            _queueManagers.Each(x => x.Dispose());
        }

        public IQueueManager ManagerFor(IPEndPoint endpoint)
        {
            return _queueManagers[endpoint];
        }

        public void Start(IEnumerable<RhinoUri> uriList)
        {
            uriList.GroupBy(x => x.Endpoint).Each(group => {
                string[] queueNames = group.Select(x => x.QueueName).ToArray();

                var queueManager = _queueManagers[@group.Key];
                queueManager.CreateQueues(queueNames);
                queueManager.CreateQueues(RhinoQueuesTransport.DelayedQueueName);
                
                queueManager.Start();
            });
        }

        public void CreateQueue(RhinoUri uri)
        {
            _queueManagers[uri.Endpoint].CreateQueues(uri.QueueName);
        }

        public IEnumerable<Envelope> ReplayDelayed(DateTime currentTime)
        {
            return _queueManagers.SelectMany(x => ReplayDelayed(x, currentTime));
        }

        public IEnumerable<Envelope> ReplayDelayed(QueueManager queueManager, DateTime currentTime)
        {
            // Corey,
            // My thought here is within a transaction we'll move all the messages from the "Delayed" queue back to the
            // queue specified by the "Received-At" header, then return an enumerable of the Envelope's that are getting
            // moved back in for the purpose of auditing.  I'm not completely sure what the easiest thing to do w/
            // the rhino queue API is and betting you do.

            throw new NotImplementedException();
//            var queue = queueManager.GetQueue(RhinoQueuesTransport.DelayedQueueName);
//            queue.EnqueueDirectlyTo();        
        }
    }
}