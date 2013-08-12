using System.Collections.Generic;

namespace FubuTransportation.Runtime
{
    /// <summary>
    /// Models a queue of outgoing messages as a result of the current message so you don't even try to 
    /// send replies until the original message succeeds
    /// Plus giving you the ability to set the correlation identifiers
    /// </summary>
    public interface IOutgoingMessages : IEnumerable<object>
    {
        void Enqueue(object message);
    }
}