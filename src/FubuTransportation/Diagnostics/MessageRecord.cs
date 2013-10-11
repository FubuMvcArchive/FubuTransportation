using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuTransportation.Runtime;
using HtmlTags;

namespace FubuTransportation.Diagnostics
{
    public class MessageRecord : MessageRecordNode
    {
        public string Id;
        public string Message;
        public string Node;
        public string ParentId;
        public string Type;
        public string Headers;
        public string ExceptionText;

        public MessageRecord()
        {
        }

        public MessageRecord(EnvelopeToken envelope)
        {
            Id = envelope.CorrelationId;
            ParentId = envelope.ParentId;
            if (envelope.Message != null)
            {
                Type = envelope.Message.GetType().FullName;
            }

            Headers = envelope.Headers.Keys().Select(x => "{0}={1}".ToFormat(x, envelope.Headers[x])).Join(";");
        }

        public override HtmlTag ToLeafTag()
        {
            return new HtmlTag("li").Text("{0}: {1}".ToFormat(Node, Message));
        }

        public override string ToString()
        {
            return string.Format("{0} from {2}, Message: {1}, Headers: {3}", Id, Message, Node, Headers);
        }
    }
}