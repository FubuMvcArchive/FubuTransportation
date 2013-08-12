using System.Collections.Generic;

namespace FubuTransportation
{
    public interface IEventAggregator
    {
        // Sending messages
        void SendMessage<T>(T message);
        void SendMessage<T>() where T : new();

        // Explicit registration
        void AddListener(object listener);
        void RemoveListener(object listener);

        IEnumerable<object> Listeners { get; } 
    }

    
}