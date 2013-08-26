using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FubuCore;
using FubuCore.Util;
using FubuTransportation.Runtime;
using System.Linq;

namespace FubuTransportation.InMemory
{

    public static class InMemoryQueueManager
    {
        public static readonly Uri DelayedUri = "memory://localhost/delayed".ToUri();

        private static readonly Cache<Uri, InMemoryQueue> _queues = new Cache<Uri,InMemoryQueue>(x => new InMemoryQueue(x));
        private static readonly IList<Envelope> _delayed = new List<Envelope>(); 
        private static readonly ReaderWriterLockSlim _delayedLock = new ReaderWriterLockSlim();
    
        public static void ClearAll()
        {
            _delayedLock.Write(() => {
                _delayed.Clear();
            });

            
            _queues.Each(x => x.SafeDispose());
            _queues.ClearAll();
        }

        public static void AddToDelayedQueue(Envelope envelope)
        {
            _delayedLock.Write(() => {
                _delayed.Add(envelope);
            });
        }

        public static IEnumerable<Envelope> DequeueDelayedEnvelopes(DateTime currentTime)
        {
            var delayed = _delayedLock.Read(() => {
                return _delayed.Where(x => x.ExecutionTime.Value <= currentTime).ToArray();
            });

            var list = new List<Envelope>();

            foreach (Envelope envelope in delayed)
            {
                _delayedLock.Write(() => {
                    try
                    {
                        _delayed.Remove(envelope);
                        var clone = envelope.Clone();

                        _queues[clone.ReceivedAt].Enqueue(clone);

                        list.Add(clone);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                });

            }

            return list;
        } 

        public static InMemoryQueue QueueFor(Uri uri)
        {
            return _queues[uri];
        }

        public static IEnumerable<Envelope> DelayedEnvelopes()
        {
            return _delayedLock.Read(() => {
                return _delayed.ToArray();
            });
        }
    }
}