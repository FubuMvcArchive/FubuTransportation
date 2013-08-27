using FubuTransportation.Runtime;
using LightningQueues;
using LightningQueues.Model;

namespace FubuTransportation.LightningQueues
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
            _queues.MoveTo(LightningQueuesTransport.DelayedQueueName, _message);
        }
    }
}