using System;
using System.Transactions;
using FubuTransportation.Runtime;

namespace FubuTransportation.RhinoQueues
{
    public class TransactionCallback : IMessageCallback
    {
        private readonly TransactionScope _transaction;

        public TransactionCallback(TransactionScope transaction)
        {
            _transaction = transaction;
        }

        public void MarkSuccessful()
        {
            _transaction.Complete();
        }

        public void MarkFailed()
        {
            throw new NotImplementedException();
        }
    }
}