using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Transactions;
using System.Collections.Generic;
using FubuCore;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using Rhino.Queues;
using Rhino.Queues.Model;
using System.Linq;

namespace FubuTransportation.RhinoQueues
{

    public class RhinoQueuesChannel : IChannel
    {
        private readonly Uri _address;
        private readonly int _port;
        private IPEndPoint _endpoint;
        private string _queueName;

        // TODO -- needs to validate the Uri
        public RhinoQueuesChannel(Uri address)
        {
            _address = address;

            // "rhino.queues://localhost:2424/client_management_bus_local"

            var first = _address.Segments.First();
            var parts = first.Split(':');
            if (parts.Length == 2)
            {
                _port = int.Parse(parts.Last());
                if (parts.First().EqualsIgnoreCase("localhost"))
                {
                    _endpoint = new IPEndPoint(IPAddress.Loopback, _port);
                }
            }
            else
            {
                // TODO -- throw something?
                // 
            }
            
            _queueName = _address.Segments.Last();
            

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Uri Address { get { return _address; } }
        public void StartReceiving(ChannelOptions options, IReceiver receiver)
        {
            throw new NotImplementedException();
        }
    }



    public class RhinoQueuesTransport : ITransport
    {
        private readonly List<Thread> _threads;
        private readonly RhinoQueuesSettings _settings;
        private readonly IPersistentQueue _queue;
        private bool _disposed;

        public RhinoQueuesTransport(RhinoQueuesSettings settings, IPersistentQueue queue)
        {
            _settings = settings;
            _queue = queue;
            _threads = new List<Thread>();
            Address = new Uri("rhino.queues://{0}:{1}/".ToFormat(Environment.MachineName, settings.Port));
        }

        public Uri Address { get; private set; }

        public int ThreadCount { get { return _threads.Count; }}

//        public void Send(Uri destination, Envelope envelope)
//        {
//            //TODO delayed messages
//            // TODO -- pull out a factory method for our Envelope to RhinoQueues Message & UT
//            var messagePayload = new MessagePayload
//            {
//                Data = envelope.Data, 
//                Headers = envelope.Headers
//            };
//            _queue.Send(destination, messagePayload);
//        }

        public bool Matches(Uri uri)
        {
            return uri.Scheme.EqualsIgnoreCase("rhino.queues");
        }

        public void StartReceiving(ChannelOptions options, IReceiver receiver)
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