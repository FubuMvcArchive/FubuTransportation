using System;
using System.Collections.Specialized;
using FubuCore.Util;
using FubuMVC.Core.Http;

namespace FubuTransportation.Runtime
{
    public class Envelope
    {
        public static readonly string Id = "Id";
        public static readonly string OriginalId = "OriginalId";
        public static readonly string ParentId = "ParentId";
        public static readonly string ContentType = HttpResponseHeaders.ContentType;

        public Envelope(IMessageCallback callback, NameValueCollection headers = null)
        {
            Callback = callback;
            Headers = headers ?? new NameValueCollection();
        }

        public NameValueCollection Headers { get; private set; } 

        public object Message;
        public byte[] Data;

        // TODO -- do routing slip tracking later
        
        public Uri Source;

        public IMessageCallback Callback;
    }


}