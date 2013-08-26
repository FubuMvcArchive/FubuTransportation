using System;
using System.Transactions;
using FubuTransportation.Runtime;
using Rhino.Queues;
using Rhino.Queues.Model;

namespace FubuTransportation.RhinoQueues
{
    public class TransactionCallback : IMessageCallback
    {
        private readonly ITransactionalScope _transaction;
        private readonly Message _message;
        private readonly IQueueManager _queues;

        public TransactionCallback(ITransactionalScope transaction, Message message, IQueueManager queues)
        {
            _transaction = transaction;
            _message = message;
            _queues = queues;
        }

        public void MarkSuccessful()
        {
            _transaction.Commit();
        }

        public void MarkFailed()
        {
            _transaction.Rollback();
        }

        public void MoveToDelayed()
        {
            _queues.MoveTo(RhinoQueuesTransport.DelayedQueueName, _message);
        }
    }
}