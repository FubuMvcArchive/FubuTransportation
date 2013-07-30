using System;
using System.Collections.Generic;
using System.Threading;
using System.Transactions;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using Rhino.Queues;

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
            throw new NotImplementedException();
        }

        public RhinoQueuesChannel(Uri address, string queueName, IQueueManager queueManager)
        {
            _address = address;
            _queueName = queueName;
            _queueManager = queueManager;
        }

        public Uri Address { get { return _address; } }

        public void StartReceiving(ChannelNode node, IReceiver receiver)
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
                using (var tx = new TransactionScope(TransactionScopeOption.Required,
                                                     new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
                {
                    var message = _queueManager.Receive(queueName);

                    // TODO -- pull out a factory method for RhinoQueues.Message to our Envelope & UT
                    var envelope = new Envelope(new TransactionCallback(tx), message.Headers)
                    {
                        Data = message.Data
                    };

                    receiver.Receive(this, envelope);
                }
            }
        }

        public void Dispose()
        {
            _disposed = true;
            //queues is already disposed by container

            foreach (var thread in _threads)
            {
                thread.Join();
            }
        }

        //        public void Send(Uri destination, Envelope envelope)
        //        {
        //            //TODO delayed messages
        //            // TODO -- pull out a factory method for our Envelope to RhinoQueues Message & UT
        //            var messagePayload = new MessagePayload
        //            {
        //                Data = envelope.Data, 
        //                Headers = envelope.Headers
        //            };
        //            _queues.Send(destination, messagePayload);
        //        }
    }
}