using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Util;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using System.Linq;
using System.Collections.Generic;

namespace FubuTransportation.InMemory
{

    public static class InMemoryQueueManager
    {
        private static readonly Cache<Uri, InMemoryQueue> _queues = new Cache<Uri,InMemoryQueue>(x => new InMemoryQueue(x));

    
        public static void ClearAll()
        {
            _queues.Each(x => x.SafeDispose());
            _queues.ClearAll();
        }

        public static InMemoryQueue QueueFor(Uri uri)
        {
            return _queues[uri];
        }
    }



    public class InMemoryQueue : IDisposable
    {
        private readonly Uri _uri;
        private readonly BlockingCollection<byte[]> _queue = new BlockingCollection<byte[]>(new ConcurrentBag<byte[]>());
        private readonly BinaryFormatter _formatter;
        private readonly IList<Listener> _listeners = new List<Listener>(); 

        public InMemoryQueue(Uri uri)
        {
            _uri = uri;
            _formatter = new BinaryFormatter();
        }

        public Uri Uri
        {
            get { return _uri; }
        }

        public void Enqueue(Envelope envelope)
        {
            using (var stream = new MemoryStream())
            {
                _formatter.Serialize(stream, envelope);

                stream.Position = 0;
                var bytes = stream.ReadAllBytes();

                _queue.Add(bytes);
            }
        }



        public void Dispose()
        {
            _queue.CompleteAdding();
            _listeners.Each(x => x.SafeDispose());
        }

        public void AddListener(IReceiver receiver)
        {
            var listener = new Listener(this, receiver);
            _listeners.Add(listener);

            listener.Start();
        }

        public class Listener : IDisposable
        {
            private readonly InMemoryQueue _parent;
            private readonly IReceiver _receiver;
            private bool _disposed;
            private Task _task;

            public Listener(InMemoryQueue parent, IReceiver receiver)
            {
                _parent = parent;
                _receiver = receiver;
            }

            public void Start()
            {
                _task = Task.Factory.StartNew(() => {
                    foreach (byte[] data in _parent._queue.GetConsumingEnumerable())
                    {
                        if (_disposed)
                        {
                            break;
                        }

                        using (var stream = new MemoryStream(data))
                        {
                            var envelope = _parent._formatter.Deserialize(stream).As<Envelope>();
                            _receiver.Receive(envelope);
                        }
                    }
                });


            }

            public void Dispose()
            {
                _disposed = true;
                _task.SafeDispose();
            }
        }
    }

    public class InMemoryTransport : ITransport
    {
        public void Dispose()
        {
            // nothing
        }

        public void OpenChannels(ChannelGraph graph)
        {
            graph.Where(x => x.Protocol() == InMemoryChannel.Protocol).Each(x => x.Channel = new InMemoryChannel(x));
        }
    }

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

        public void Send(Envelope envelope)
        {
            _queue.Enqueue(envelope);
        }
    }
}