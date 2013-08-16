using System;

namespace FubuTransportation.InMemory
{
    public interface ISagaStateCache<T> where T : class
    {
        void Store(Guid correlationId, T state);
        T Find(Guid correlationId);
        void Delete(Guid correlationId);
    }
}