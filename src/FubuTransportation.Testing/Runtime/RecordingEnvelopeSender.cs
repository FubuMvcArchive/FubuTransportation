using System.Collections.Generic;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Cascading;

namespace FubuTransportation.Testing.Runtime
{
    public class RecordingEnvelopeSender : IEnvelopeSender, IOutgoingSender
    {
        public readonly IList<Envelope> Sent = new List<Envelope>(); 
        public readonly IList<object> Outgoing = new List<object>(); 

        public string Send(Envelope envelope)
        {
            Sent.Add(envelope);

            return envelope.CorrelationId;
        }

        public void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages)
        {
            Outgoing.AddRange(cascadingMessages);
        }

        public void SendFailureAcknowledgement(string message)
        {
            FailureAcknowledgementMessage = message;
        }

        public string FailureAcknowledgementMessage { get; set; }
    }
}