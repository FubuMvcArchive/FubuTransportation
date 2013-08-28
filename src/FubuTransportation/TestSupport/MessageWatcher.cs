using System;
using System.Diagnostics;
using Bottles.Services.Messaging.Tracking;
using FubuTransportation.Logging;
using FubuCore;
using FubuTransportation.Runtime;

namespace FubuTransportation.TestSupport
{
    public class MessageWatcher : IListener
        , IListener<ChainExecutionStarted>
        , IListener<ChainExecutionFinished>
        , IListener<EnvelopeSent>
        , IListener<MessageSuccessful>
        , IListener<MessageFailed>
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

        public void Handle(EnvelopeSent message)
        {
            handle(message.Envelope, MessageTrack.Sent, message.Uri);
        }

        private void handle(EnvelopeToken envelope, string status, Uri uri)
        {
            MessageHistory.Record(new MessageTrack
            {
                Type = "OutstandingEnvelope",
                Id = envelope.CorrelationId,
                FullName = "{0}@{1}".ToFormat(envelope.CorrelationId, uri),
                Status = status
            });
        }

        public void Handle(MessageSuccessful message)
        {
            handle(message.Envelope, MessageTrack.Received, message.Envelope.Destination);
        }

        public void Handle(MessageFailed message)
        {
            handle(message.Envelope, MessageTrack.Received, message.Envelope.Destination);
        }
    }


}