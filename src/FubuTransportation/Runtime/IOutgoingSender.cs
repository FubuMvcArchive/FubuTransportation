using System.Collections.Generic;

namespace FubuTransportation.Runtime
{
    public interface IOutgoingSender
    {
        void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages);
    }
}