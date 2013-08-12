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
        public static readonly string ReplyRequested = "Reply-Requested";
        public static readonly string Response = "Response";
        public static readonly string DestinationKey = "Destination";

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

        // TODO -- this is where the routing slip is going to come into place
        public Envelope ForResponse(object message)
        {
            var child = new Envelope
            {
                Message = message,
                OriginalId = OriginalId ?? CorrelationId,
                ParentId = CorrelationId
            };

            if (Headers.Has(ReplyRequested) && Headers[ReplyRequested].EqualsIgnoreCase("true"))
            {
                child.Headers[Response] = true.ToString();
                child.Destination = Source;
            }

            return child;
        }
    }
}