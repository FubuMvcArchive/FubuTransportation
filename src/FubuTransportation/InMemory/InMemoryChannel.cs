using System;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;

namespace FubuTransportation.InMemory
{
    public class InMemoryChannel : IChannel
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
            
        }

        public Uri Address { get; private set; }
        public void StartReceiving(IReceiver receiver, ChannelNode node)
        {
            for (int i = 0; i < node.ThreadCount; i++)
            {
                _queue.AddListener(receiver);              
            }
        }

        public void Send(byte[] data, IHeaders headers)
        {
            var envelope = new Envelope(headers) {Data = data};
            _queue.Enqueue(envelope);
        }
    }
}