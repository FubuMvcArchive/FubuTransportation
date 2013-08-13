using System.Diagnostics;
using Bottles.Services.Messaging.Tracking;
using FubuTransportation.Logging;

namespace FubuTransportation.TestSupport
{
    public class MessageWatcher : IListener, IListener<ChainExecutionStarted>, IListener<ChainExecutionFinished>
    {
        public static readonly string MessageTrackType = "Handler Chain Execution";

        public void Handle(ChainExecutionStarted message)
        {
            var track = MessageTrack.ForSent(message, message.Envelope.CorrelationId);
            track.Type = track.FullName = MessageTrackType;

            MessageHistory.Record(track);
        }

        public void Handle(ChainExecutionFinished message)
        {
            var track = MessageTrack.ForReceived(message, message.Envelope.CorrelationId);
            track.Type = track.FullName = MessageTrackType;

            MessageHistory.Record(track);
        }
    }


}