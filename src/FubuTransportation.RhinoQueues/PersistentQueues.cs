using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Transactions;
using FubuCore.Dates;
using FubuCore.Logging;
using FubuCore.Util;
using FubuTransportation.Runtime;
using Rhino.Queues;

namespace FubuTransportation.RhinoQueues
{
    public class PersistentQueues : IPersistentQueues
    {
        private readonly ILogger _logger;
        // TODO -- Corey, does this need to be unique per endpoint??????
        public const string EsentPath = "fubutransportation.esent";

        private readonly Cache<IPEndPoint, QueueManager> _queueManagers =
            new Cache<IPEndPoint, QueueManager>(ip => new QueueManager(ip, EsentPath));

        public PersistentQueues(ILogger logger)
        {
            _logger = logger;
        }

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
            var list = new List<Envelope>();

            var transactionalScope = queueManager.BeginTransactionalScope();


            try
            {
                var messages = queueManager.GetQueue(RhinoQueuesTransport.DelayedQueueName).GetAllMessages(null)
                    .Where(x => x.ExecutionTime() <= currentTime).ToArray();

                messages.Each(msg => {
                    var uri = msg.Headers[Envelope.ReceivedAtKey].ToRhinoUri();
                    queueManager.MoveTo(uri.QueueName, msg);

                    list.Add(new Envelope());
                });

                transactionalScope.Commit();
            }
            catch (Exception e)
            {
                transactionalScope.Rollback();
                _logger.Error("Error trying to move delayed messages back to the original queue", e);
            }

            return list;
        }
    }
}