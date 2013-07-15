using System;
using System.Collections.Specialized;

namespace FubuTransportation.Runtime
{
    public class Envelope
    {
        public static readonly string Id = "Id";
        public static readonly string OriginalId = "OriginalId";
        public static readonly string ParentId = "ParentId";

        private readonly NameValueCollection _headers = new NameValueCollection();

        public Envelope(params object[] messages)
        {
            Messages = messages;
        }

        public NameValueCollection Headers
        {
            get { return _headers; }
        }

        public object[] Messages;
        public Uri Source;
        public Uri Destination;

        public Envelope Originator { get; set; }

        // TODO -- needs to track where it came from so we can do the return on request/reply


        //public DateTimeOffset? DeliverBy { get; set; }

        /// <summary>
        /// The destination the messages will be sent to.  This may be null if the 
        /// messages are being sent to multiple endpoints.
        /// </summary>
        //public Endpoint Destination { get; set; }

        //public NameValueCollection Headers { get; set; }
        //public int? MaxAttempts { get; set; }


        /// <summary>
        /// The current endpoint.  This may be null on a one-way bus.
        /// </summary>
        //public Endpoint Source { get; set; }
    }
}