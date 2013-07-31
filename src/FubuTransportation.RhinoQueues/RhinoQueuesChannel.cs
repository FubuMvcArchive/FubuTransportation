using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Transactions;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using Rhino.Queues;
using Rhino.Queues.Model;

namespace FubuTransportation.RhinoQueues
{
    public class RhinoQueuesChannel : IChannel
    {
        private readonly Uri _address;
        private readonly string _queueName;
        private readonly IQueueManager _queueManager;
        private bool _disposed;
        private readonly List<Thread> _threads = new List<Thread>();

        public static RhinoQueuesChannel Build(RhinoUri uri, IPersistentQueues queues)
        {
            var queueManager = queues.ManagerFor(uri.Endpoint);
            return new RhinoQueuesChannel(uri.Address, uri.QueueName, queueManager);
        }

        public RhinoQueuesChannel(Uri address, string queueName, IQueueManager queueManager)
        {
            _address = address;
            _queueName = queueName;
            _queueManager = queueManager;
        }

        public Uri Address { get { return _address; } }

        public void StartReceiving(IReceiver receiver, ChannelNode node)
        {
            startListeningThreads(_queueName, node.ThreadCount, receiver);
        }

        private void startListeningThreads(string queueName, int threadCount, IReceiver receiver)
        {
            for (int i = 0; i < threadCount; ++i)
            {
                var thread = new Thread(() => startListeningOnQueue(queueName, receiver))
                {
                    IsBackground = true,
                    Name = "FubuTransportation.RhinoQueuesTransport Receiving Thread",
                };
                thread.Start();
                _threads.Add(thread);
            }
        }

        private void startListeningOnQueue(string queueName, IReceiver receiver)
        {
            while (!_disposed)
            {
                var transactionalScope = _queueManager.BeginTransactionalScope();
                var message = transactionalScope.Receive(queueName);

                var envelope = ToEnvelope(transactionalScope, message);

                receiver.Receive(envelope);
            }

            Debug.WriteLine("I'm done on this thread");
        }

        public static Envelope ToEnvelope(ITransactionalScope tx, Message message)
        {
            var envelope = new Envelope(new TransactionCallback(tx), message.Headers)
            {
                Data = message.Data
            };

            envelope.Headers[Envelope.Id] = message.Id.MessageIdentifier.ToString();

            return envelope;
        }

        public void Dispose()
        {
            _disposed = true;
            //queues is already disposed by container

            foreach (var thread in _threads)
            {
                
                if (!thread.Join(2000))
                {
                    thread.Abort();
                }
            }
        }

        public void Send(Envelope envelope)
        {
            //TODO delayed messages
            // TODO -- pull out a factory method for our Envelope to RhinoQueues Message & UT
            var messagePayload = new MessagePayload
            {
                Data = envelope.Data,
                Headers = envelope.Headers
            };

            //TODO Should this scope be shared with the dequeue scope?
            var sendingScope = _queueManager.BeginTransactionalScope();
            var id = sendingScope.Send(_address, messagePayload);
            envelope.CorrelationId = id.MessageIdentifier;
            sendingScope.Commit();

        }
    }
}