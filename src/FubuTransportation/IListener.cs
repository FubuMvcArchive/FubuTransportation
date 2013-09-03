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
        DateTime? ExpiresAt { get; }
    }

    public static class ExpiringListenerExtensions
    {
        public static bool IsExpired(this object o, DateTime currentTime)
        {
            var expiring = o as IExpiringListener;

            if (expiring == null) return false;

            return expiring.IsExpired || (expiring.ExpiresAt.HasValue && expiring.ExpiresAt.Value <= currentTime);
        }
    }
}