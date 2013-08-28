using System;
using FubuTransportation.Runtime.Headers;

namespace FubuTransportation.Runtime
{
    [Serializable]
    public class EnvelopeToken : HeaderWrapper
    {
        public EnvelopeToken()
        {
            Headers = new NameValueHeaders();
            CorrelationId = Guid.NewGuid().ToString();
        }

        public byte[] Data;
        public Lazy<object> MessageSource
        {
            set
            {
                _message = value;
            }
        }

        [NonSerialized]
        private Lazy<object> _message;

        public object Message
        {
            get { return _message == null ? null : _message.Value; }
            set
            {
                _message = new Lazy<object>(() => value);
            }
        }

        protected bool Equals(EnvelopeToken other)
        {
            return Equals(Data, other.Data) && Equals(Message, other.Message) && Equals(Headers, other.Headers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Envelope)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Data != null ? Data.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_message != null ? _message.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Headers != null ? Headers.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}