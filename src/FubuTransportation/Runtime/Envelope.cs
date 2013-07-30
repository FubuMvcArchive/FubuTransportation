using System;
using System.Collections.Specialized;
using FubuMVC.Core.Http;

namespace FubuTransportation.Runtime
{
    public class Envelope
    {
        public static readonly string Id = "Id";
        public static readonly string OriginalId = "OriginalId";
        public static readonly string ParentId = "ParentId";
        public static readonly string ContentTypeKey = HttpResponseHeaders.ContentType;
        public IMessageCallback Callback;

        public byte[] Data;
        public object Message;

        // TODO -- do routing slip tracking later

        public Uri Source;

        public Envelope(IMessageCallback callback, NameValueCollection headers = null)
        {
            Callback = callback;
            Headers = headers ?? new NameValueCollection();
        }

        public string ContentType
        {
            get { return Headers.Get(ContentTypeKey); }
            set
            {
                Headers.Set(ContentTypeKey, value);
            }
        }

        public NameValueCollection Headers { get; private set; }

        // TODO -- get this on the receive too!
        public Guid CorrelationId { get; set; }
    }
}