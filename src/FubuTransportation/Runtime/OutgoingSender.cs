using System.Collections.Generic;
using FubuCore;

namespace FubuTransportation.Runtime
{
    public class OutgoingSender : IOutgoingSender
    {
        private readonly IEnvelopeSender _sender;

        public OutgoingSender(IEnvelopeSender sender)
        {
            _sender = sender;
        }

        public void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages)
        {
            cascadingMessages.Each(o => SendOutgoingMessage(original, o));
        }

        public void SendOutgoingMessage(Envelope original, object o)
        {
            var cascadingEnvelope = o is ISendMyself
                ? o.As<ISendMyself>().CreateEnvelope(original)
                : original.ForResponse(o);

            _sender.Send(cascadingEnvelope);
        }
    }

    public interface ISendMyself
    {
        Envelope CreateEnvelope(Envelope original);
    }
}