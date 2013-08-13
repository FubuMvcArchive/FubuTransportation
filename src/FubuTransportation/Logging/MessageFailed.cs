using System;
using FubuCore.Logging;
using FubuTransportation.Runtime;

namespace FubuTransportation.Logging
{
    public class MessageFailed : LogRecord
    {
        public Envelope Envelope { get; set; }
        public Exception Exception { get; set; }

        protected bool Equals(MessageFailed other)
        {
            return Equals(Envelope, other.Envelope);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MessageFailed) obj);
        }

        public override int GetHashCode()
        {
            return (Envelope != null ? Envelope.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("Message failed: {0}", Envelope);
        }
    }
}