using System;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Headers;

namespace FubuTransportation.InMemory
{
    public class InMemoryChannel : IChannel, IDisposable
    {
        public static readonly string Protocol = "memory";
        private readonly InMemoryQueue _queue;

        public InMemoryChannel(ChannelNode node)
        {
            Address = node.Uri;
            _queue = InMemoryQueueManager.QueueFor(Address);
        }

        public void Dispose()
        {
            _queue.Dispose();
        }

        public Uri Address { get; private set; }
        public void Receive(IReceiver receiver)
        {
            _queue.Receive(receiver);
        }

        public void Send(byte[] data, IHeaders headers)
        {
            var envelope = new EnvelopeToken
            {
                Data = data,
                Headers = headers
            };

            _queue.Enqueue(envelope);
        }
    }
}