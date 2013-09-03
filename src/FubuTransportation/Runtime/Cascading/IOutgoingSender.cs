using System.Collections.Generic;

namespace FubuTransportation.Runtime.Cascading
{
    public interface IOutgoingSender
    {
        void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages);
    }
}