using System;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Headers;
using LightningQueues;

namespace FubuTransportation.LightningQueues
{
    public class LightningQueuesReplyChannel : IChannel
    {
        private readonly IQueueManager _queueManager;

        public LightningQueuesReplyChannel(Uri destination, IQueueManager queueManager)
        {
            _queueManager = queueManager;
            Address = destination.ToMachineUri();
        }

        public void Dispose()
        {
        }

        public Uri Address { get; private set; }
        public ReceivingState Receive(IReceiver receiver)
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] data, IHeaders headers)
        {
            var payload = new MessagePayload {Data = data, Headers = headers.ToNameValues()};
            var scope = _queueManager.BeginTransactionalScope();
            scope.Send(Address, payload);
            scope.Commit();
        }
    }
}