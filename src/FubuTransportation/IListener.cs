using System;

namespace FubuTransportation
{
    public interface IListener<T>
    {
        void Handle(T message);
    }

    /// <summary>
    /// Marker interface for types that will listen to the EventAggregator 
    /// at application start up time
    /// </summary>
    public interface IListener
    {
        
    }

    public interface IExpiringListener
    {
        bool IsExpired { get; }
        DateTime ExpiresAt { get; }
    }
}