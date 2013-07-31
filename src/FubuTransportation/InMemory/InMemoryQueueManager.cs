using System;
using FubuCore;
using FubuCore.Util;

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
}