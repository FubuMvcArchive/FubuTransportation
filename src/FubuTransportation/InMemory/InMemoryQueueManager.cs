using System;
using System.Collections.Generic;
using FubuCore;
using FubuCore.Util;

namespace FubuTransportation.InMemory
{

    public static class InMemoryQueueManager
    {
        public static readonly Uri DelayedUri = "memory://localhost/delayed".ToUri();

        private static readonly Cache<Uri, InMemoryQueue> _queues = new Cache<Uri,InMemoryQueue>(x => new InMemoryQueue(x));

    
        public static void ClearAll()
        {
            _queues.Each(x => x.SafeDispose());
            _queues.ClearAll();
        }

        public static InMemoryQueue DelayedQueue()
        {
            return QueueFor(DelayedUri);
        }

        public static InMemoryQueue QueueFor(Uri uri)
        {
            return _queues[uri];
        }
    }
}