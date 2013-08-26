using System;
using FubuMVC.Core.Http;
using FubuCore;

namespace FubuTransportation.Runtime
{
    [Serializable]
    public class Envelope
    {
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

        public byte[] Data;

        [NonSerialized] public object Message;

        // TODO -- do routing slip tracking later

        public Envelope(IHeaders headers)
        {
            Headers = headers;
        }

        public Envelope()
        {
            Headers = new NameValueHeaders();
            CorrelationId = Guid.NewGuid().ToString();
        }

        public Uri Source
        {
            get { return Headers[SourceKey].ToUri(); }
            set { Headers[SourceKey] = value == null ? null : value.ToString(); }
        }

        public Uri ReplyUri
        {
            get { return Headers[ReplyUriKey].ToUri(); }
            set { Headers[ReplyUriKey] = value == null ? null : value.ToString(); }
        }

        public string ContentType
        {
            get { return Headers[ContentTypeKey]; }
            set { Headers[ContentTypeKey] = value; }
        }

        public string OriginalId
        {
            get { return Headers[OriginalIdKey]; }
            set { Headers[OriginalIdKey] = value; }
        }

        public string ParentId
        {
            get { return Headers[ParentIdKey]; }
            set { Headers[ParentIdKey] = value; }
        }

        public string ResponseId
        {
            get { return Headers[ResponseIdKey]; }
            set { Headers[ResponseIdKey] = value; }
        }

        public Uri Destination
        {
            get { return Headers[DestinationKey].ToUri(); }
            set { Headers[DestinationKey] = value == null ? null : value.ToString(); }
        }

        public IHeaders Headers { get; private set; }

        public string CorrelationId
        {
            get
            {
                return Headers[IdKey];
            }
            set { Headers[IdKey] = value; }
        }

        public bool ReplyRequested
        {
            get { return Headers.Has(ReplyRequestedKey) ? Headers[ReplyRequestedKey].EqualsIgnoreCase("true") : false; }
            set
            {
                if (value)
                {
                    Headers[ReplyRequestedKey] = "true";
                }
                else
                {
                    Headers.Remove(ReplyRequestedKey);
                }
            }
        }

        public DateTime? ExecutionTime
        {
            get { return Headers.Has(ExecutionTimeKey) ? DateTime.Parse(Headers[ExecutionTimeKey]) : (DateTime?) null; }
            set
            {
                if (value == null)
                {
                    Headers.Remove(ExecutionTimeKey);
                }
                else
                {
                    Headers[ExecutionTimeKey] = value.Value.ToString();
                }
                
            }
        }

        // TODO -- this is where the routing slip is going to come into place
        public Envelope ForResponse(object message)
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
    }
}