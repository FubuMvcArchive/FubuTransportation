using System.Collections.Generic;
using System.Linq;
using System.Net;
using FubuCore.Util;
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
                queueManager.Start();
            });
        }
    }
}