using System.Collections;
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

    public class OutgoingMessages : IOutgoingMessages
    {
        private readonly IList<object> _messages = new List<object>();

        // TODO -- needs to track the originating message in order to do the request/replay semantics

        public void Enqueue(object message)
        {
            var enumerable = message as IEnumerable<object>;
            if (enumerable == null)
            {
                _messages.Add(message);
            }
            else
            {
                _messages.AddRange(enumerable);
            }
        }

        public IEnumerator<object> GetEnumerator()
        {
            return _messages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}