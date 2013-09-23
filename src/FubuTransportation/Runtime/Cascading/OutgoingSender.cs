using System.Collections.Generic;
using FubuCore;

namespace FubuTransportation.Runtime.Cascading
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
            if (original.AckRequested)
            {
                sendAcknowledgement(original);
            }

            cascadingMessages.Each(o => SendOutgoingMessage(original, o));
        }

        public void SendFailureAcknowledgement(Envelope original, string message)
        {
            if (original.AckRequested || original.ReplyRequested)
            {
                var envelope = new Envelope
                {
                    Destination = original.ReplyUri,
                    ResponseId = original.CorrelationId,
                    Message = new FailureAcknowledgement()
                    {
                        CorrelationId = original.CorrelationId, 
                        Message = message
                    }
                };

                _sender.Send(envelope);
            }
        }

        private void sendAcknowledgement(Envelope original)
        {
            var envelope = new Envelope
            {
                Destination = original.ReplyUri,
                ResponseId = original.CorrelationId,
                Message = new Acknowledgement {CorrelationId = original.CorrelationId}
            };

            _sender.Send(envelope);
        }

        public void SendOutgoingMessage(Envelope original, object o)
        {
            var cascadingEnvelope = o is ISendMyself
                ? o.As<ISendMyself>().CreateEnvelope(original)
                : original.ForResponse(o);

            _sender.Send(cascadingEnvelope);
        }
    }
}