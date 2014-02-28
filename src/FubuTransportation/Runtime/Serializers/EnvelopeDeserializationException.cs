using System;
using System.Runtime.Serialization;

namespace FubuTransportation.Runtime.Serializers
{
    [Serializable]
    public class EnvelopeDeserializationException : Exception
    {
        public EnvelopeDeserializationException(string message) : base(message)
        {
        }

        public EnvelopeDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}