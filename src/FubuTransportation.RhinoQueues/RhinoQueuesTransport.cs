using System;
using System.IO;
using System.Threading;
using System.Transactions;
using System.Collections.Generic;
using FubuCore;
using FubuTransportation.Runtime;
using Rhino.Queues;
using Rhino.Queues.Model;

namespace FubuTransportation.RhinoQueues
{
    public class RhinoQueuesTransport : ITransport
    {
        private readonly List<Thread> _threads;
        private readonly RhinoQueuesSettings _settings;
        private readonly IMessageSerializer _serializer;
        private readonly IPersistentQueue _queue;
        private bool _disposed;

        public RhinoQueuesTransport(RhinoQueuesSettings settings, IMessageSerializer serializer, IPersistentQueue queue)
        {
            _settings = settings;
            _serializer = serializer;
            _queue = queue;
            _threads = new List<Thread>();
            Id = new Uri("rhino.queues://{0}:{1}/".ToFormat(Environment.MachineName, settings.Port));
        }

        public Uri Id { get; private set; }

        public int ThreadCount { get { return _threads.Count; }}

        public void Send(Uri destination, Envelope envelope)
        {
            //TODO delayed messages
            // TODO -- pull out a factory method for our Envelope to RhinoQueues Message & UT
            var messagePayload = new MessagePayload
            {
                Data = envelope.Data, 
                Headers = envelope.Headers
            };
            _queue.Send(destination, messagePayload);
        }

        public bool Matches(Uri uri)
        {
            return uri.Scheme.EqualsIgnoreCase("rhino.queues");
        }

        public void StartReceiving(IReceiver receiver)
        {
            _queue.Start();
            _settings.Queues.Each(x => startListeningThreads(x.QueueName, x.ThreadCount, receiver));
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
                    new TransactionOptions{ IsolationLevel = IsolationLevel.ReadCommitted}))
                {
                    var message = _queue.Receive(queueName);

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
            //queue is already disposed by container

            foreach (var thread in _threads)
            {
                thread.Join();
            }
        }
    }
}