using System;
using System.Collections.Specialized;
using FubuTransportation.Runtime;
using Rhino.Queues;

namespace FubuTransportation.RhinoQueues
{
    public class RhinoQueuesEnvelope : Envelope
    {
        private readonly NameValueCollection _headers;
        private readonly Action _success;
        private readonly Action _failed;

        public RhinoQueuesEnvelope(NameValueCollection headers, object[] messages, Action success, Action failed)
        {
            _headers = headers;
            _success = success;
            _failed = failed;
            Messages = messages;
        }

        public override NameValueCollection Headers
        {
            get { return _headers; }
        }

        public override void MarkSuccessful()
        {
            _success();
        }

        public override void MarkFailed()
        {
            _failed();
        }
    }
}