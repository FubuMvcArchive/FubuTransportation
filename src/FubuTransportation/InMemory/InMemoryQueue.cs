using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using FubuCore;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Runtime;
using System.Linq;
using FubuTransportation.Runtime.Delayed;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.InMemory
{
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

        public void Enqueue(EnvelopeToken envelope)
        {
            using (var stream = new MemoryStream())
            {
                _formatter.Serialize(stream, envelope);

                stream.Position = 0;
                var bytes = stream.ReadAllBytes();

                _queue.Add(bytes);
            }
        }

        public IEnumerable<Envelope> Peek()
        {
            return _queue.ToArray().Select(x => _formatter.Deserialize(new MemoryStream(x)).As<Envelope>());
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
                            var token = _parent._formatter.Deserialize(stream).As<EnvelopeToken>();

                            var callback = new InMemoryCallback(_parent, token);

                            _receiver.Receive(token.Data, token.Headers, callback);
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

    public class InMemoryCallback : IMessageCallback
    {
        private readonly InMemoryQueue _parent;
        private readonly EnvelopeToken _token;

        public InMemoryCallback(InMemoryQueue parent, EnvelopeToken token)
        {
            _parent = parent;
            _token = token;
        }

        public void MarkSuccessful()
        {
            // nothing
        }

        public void MarkFailed()
        {
            Debug.WriteLine("Message was marked as failed!");
        }

        public void MoveToDelayedUntil(DateTime time)
        {
            //TODO leverage delayed message cache?
            InMemoryQueueManager.AddToDelayedQueue(_token);
        }

        public void MoveToErrors(ErrorReport report)
        {
            throw new NotImplementedException();
        }

        public void Requeue()
        {
            throw new NotImplementedException();
        }
    }
}