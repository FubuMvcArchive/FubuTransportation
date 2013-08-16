using System;
using FubuCore.Util;

namespace FubuTransportation.InMemory
{
    public class SagaStateCache<T> : ISagaStateCache<T> where T : class
    {
        private readonly Cache<Guid, T> _cache = new Cache<Guid, T>();

        public void Store(Guid correlationId, T state)
        {
            _cache[correlationId] = state;
        }

        public T Find(Guid correlationId)
        {
            return _cache.Has(correlationId) ? _cache[correlationId] : null;
        }

        public void Delete(Guid correlationId)
        {
            _cache.Remove(correlationId);
        }
    }
}