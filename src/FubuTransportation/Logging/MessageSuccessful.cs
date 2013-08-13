using FubuCore.Logging;
using FubuTransportation.Runtime;

namespace FubuTransportation.Logging
{
    public class MessageSuccessful : LogRecord
    {
        public Envelope Envelope { get; set; }

        protected bool Equals(MessageSuccessful other)
        {
            return Equals(Envelope, other.Envelope);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MessageSuccessful) obj);
        }

        public override int GetHashCode()
        {
            return (Envelope != null ? Envelope.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("Message succeeded: {0}", Envelope);
        }
    }
}