using System;
using System.Transactions;
using FubuTransportation.Runtime;
using Rhino.Queues;

namespace FubuTransportation.RhinoQueues
{
    public class TransactionCallback : IMessageCallback
    {
        private readonly ITransactionalScope _transaction;

        public TransactionCallback(ITransactionalScope transaction)
        {
            _transaction = transaction;
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
            throw new NotImplementedException();
        }
    }
}