using System;
using FubuTransportation.Runtime.Delayed;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Runtime.Invocation;
using LightningQueues;
using LightningQueues.Model;

namespace FubuTransportation.LightningQueues
{
    public class TransactionCallback : IMessageCallback
    {
        private readonly Message _message;
        private readonly IDelayedMessageCache<MessageId> _delayedMessages;
        private readonly ITransactionalScope _transaction;

        public TransactionCallback(ITransactionalScope transaction, Message message, IDelayedMessageCache<MessageId> delayedMessages)
        {
            _transaction = transaction;
            _message = message;
            _delayedMessages = delayedMessages;
        }

        public void MarkSuccessful()
        {
            _transaction.Commit();
        }

        public void MarkFailed()
        {
            _transaction.Rollback();
        }

        public void MoveToDelayedUntil(DateTime time)
        {
            _delayedMessages.Add(_message.Id, time);
            _transaction.EnqueueDirectlyTo(LightningQueuesTransport.DelayedQueueName, _message.ToPayload());
        }

        public void MoveToErrors(ErrorReport report)
        {
            throw new System.NotImplementedException();
        }

        public void Requeue()
        {
            throw new System.NotImplementedException();
        }
    }
}