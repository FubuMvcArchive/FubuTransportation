using System;
using System.ComponentModel;
using FubuCore.Dates;
using FubuCore.Logging;
using FubuTransportation.Runtime.Delayed;

namespace FubuTransportation.Runtime.Invocation
{
    [Description("Delayed Message Handler")]
    public class DelayedMessageHandler : SimpleEnvelopeHandler
    {
        private readonly ISystemTime _systemTime;

        public DelayedMessageHandler(ISystemTime systemTime)
        {
            _systemTime = systemTime;
        }

        public override bool Matches(Envelope envelope)
        {
            return envelope.IsDelayed(_systemTime.UtcNow());
        }

        public override void Execute(Envelope envelope, ILogger logger)
        {
            try
            {
                envelope.Callback.MoveToDelayed();
                logger.InfoMessage(() => new DelayedEnvelopeReceived { Envelope = envelope.ToToken() });
            }
            catch (Exception e)
            {
                envelope.Callback.MarkFailed();
                logger.Error(envelope.CorrelationId, "Failed to move delayed message to the delayed message queue", e);
            }
        }
    }
}