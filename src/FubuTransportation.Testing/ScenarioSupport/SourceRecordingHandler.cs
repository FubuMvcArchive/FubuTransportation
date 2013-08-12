using System.Diagnostics;
using Bottles.Services.Messaging.Tracking;
using FubuTransportation.Runtime;

namespace FubuTransportation.Testing.ScenarioSupport
{
    public class SourceRecordingHandler
    {
        private readonly Envelope _envelope;

        public SourceRecordingHandler(Envelope envelope)
        {
            _envelope = envelope;
        }

        public void Consume(Message message)
        {
            message.Source = _envelope.Source;
            message.Envelope = _envelope;

            Debug.WriteLine("I'm done consuming {0}/{1}", message.GetType().Name, message.Id);
            MessageHistory.Record(MessageTrack.ForReceived(message, message.Id.ToString()));
        }
    }
}