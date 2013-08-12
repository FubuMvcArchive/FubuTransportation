using System.Collections;
using System.Collections.Generic;

namespace FubuTransportation.Runtime
{
    public class OutgoingMessages : IOutgoingMessages
    {
        private readonly IList<object> _messages = new List<object>();

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