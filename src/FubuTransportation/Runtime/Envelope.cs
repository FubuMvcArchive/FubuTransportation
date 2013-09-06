using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FubuMVC.Core.Http;
using FubuCore;
using FubuTransportation.Runtime.Headers;
using FubuTransportation.Runtime.Invocation;
using FubuTransportation.Runtime.Serializers;

namespace FubuTransportation.Runtime
{
    public class Envelope : HeaderWrapper
    {
        private static readonly BinaryFormatter formatter = new BinaryFormatter();

        public static readonly string OriginalIdKey = "OriginalId";
        public static readonly string IdKey = "Id";
        public static readonly string ParentIdKey = "ParentId";
        public static readonly string ContentTypeKey = HttpResponseHeaders.ContentType;
        public static readonly string SourceKey = "Source";
        public static readonly string ChannelKey = "Channel";
        public static readonly string ReplyRequestedKey = "Reply-Requested";
        public static readonly string ResponseIdKey = "Response";
        public static readonly string DestinationKey = "Destination";
        public static readonly string ReplyUriKey = "Reply-Uri";
        public static readonly string ExecutionTimeKey = "Execution-Time";
        public static readonly string ReceivedAtKey = "Received-At";
        public static readonly string AttemptsKey = "Attempts";
        public static readonly string AckRequestedKey = "Ack-Requested";

        public byte[] Data;

        [NonSerialized]
        private Lazy<object> _message;
            
            
            
        public object Message
        {
            get { return _message == null ? null : _message.Value; }   
            set
            {
                if (value == null)
                {
                    _message = null;
                }
                else
                {
                    _message = new Lazy<object>(() => value);
                }
            }
        }

        public void UseSerializer(IEnvelopeSerializer serializer)
        {
            _message = new Lazy<object>(() => serializer.Deserialize(this));
        }


        [NonSerialized] private IMessageCallback _callback;
        public int Attempts
        {
            get { return Headers.Has(AttemptsKey) ? int.Parse(Headers[AttemptsKey]) : 0; }
            set { Headers[AttemptsKey] = value.ToString(); }
        }

        // TODO -- do routing slip tracking later

        public Envelope(IHeaders headers)
        {
            Headers = headers;

            if (CorrelationId.IsEmpty())
            {
                CorrelationId = Guid.NewGuid().ToString();
            }
            
        }

        public Envelope() : this(new NameValueHeaders())
        {

        }

        public Envelope(byte[] data, IHeaders headers, IMessageCallback callback) : this(headers)
        {
            Data = data;
            Callback = callback;
        }

        public IMessageCallback Callback
        {
            get { return _callback; }
            set { _callback = value; }
        }

        

        // TODO -- this is where the routing slip is going to come into place
        public virtual Envelope ForResponse(object message)
        {
            var child = new Envelope
            {
                Message = message,
                OriginalId = OriginalId ?? CorrelationId,
                ParentId = CorrelationId
            };

            if (ReplyRequested)
            {
                child.Headers[ResponseIdKey] = CorrelationId;
                child.Destination = ReplyUri;
            }

            return child;
        }

        public override string ToString()
        {
            var id = ResponseId.IsNotEmpty()
                ? "{0} in response to {1}".ToFormat(CorrelationId, ResponseId) : CorrelationId;

            if (Message != null)
            {
                return string.Format("Envelope for type {0} w/ Id {1}", Message.GetType().Name, id);
            }
            else
            {
                return "Envelope w/ Id {0}".ToFormat(id);
            }
        }

        public Envelope Clone()
        {
            var stream = new MemoryStream();
            formatter.Serialize(stream, this);

            stream.Position = 0;

            return (Envelope) formatter.Deserialize(stream);
        }

        public EnvelopeToken ToToken()
        {
            return new EnvelopeToken
            {
                Data = Data,
                Headers = Headers,
                MessageSource = _message
            };

            
        }

        protected bool Equals(Envelope other)
        {
            return Equals(Data, other.Data) && Equals(Message, other.Message) && Equals(_callback, other._callback) && Equals(Headers, other.Headers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Envelope) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Data != null ? Data.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_message != null ? _message.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_callback != null ? _callback.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}