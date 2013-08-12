using System.Collections.Generic;
using FubuTransportation.Runtime;

namespace FubuTransportation.Testing.Runtime
{
    public class RecordingEnvelopeSender : IEnvelopeSender
    {
        public readonly IList<Envelope> Sent = new List<Envelope>(); 

        public string Send(Envelope envelope)
        {
            Sent.Add(envelope);

            return envelope.CorrelationId;
        }
    }
}