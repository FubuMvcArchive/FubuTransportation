using System;
using System.Runtime.Serialization;
using FubuCore;

namespace FubuTransportation.Runtime
{
    [Serializable]
    public class UnknownContentTypeException : Exception
    {
        public UnknownContentTypeException(string contentType) : base("Unknown content-type '{0}'".ToFormat(contentType))
        {
        }

        protected UnknownContentTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}