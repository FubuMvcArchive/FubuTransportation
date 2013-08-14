using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Logging;
using FubuTransportation.Logging;

namespace FubuTransportation.Runtime
{
    public interface IEnvelopeSender
    {
        string Send(Envelope envelope);
    }

    public class EnvelopeSender : IEnvelopeSender
    {
        private readonly ISubscriptions _router;
        private readonly IEnvelopeSerializer _serializer;
        private readonly ILogger _logger;

        public EnvelopeSender(ISubscriptions router, IEnvelopeSerializer serializer, ILogger logger)
        {
            _router = router;
            _serializer = serializer;
            _logger = logger;
        }

        public string Send(Envelope envelope)
        {
            envelope.CorrelationId = Guid.NewGuid().ToString();

            _serializer.Serialize(envelope);

            var channels = _router.FindChannels(envelope).ToArray();

            // TODO -- needs more work and thought here about what to do.
            if (!channels.Any())
            {
                throw new Exception("No channels match this message");
            }

            // TODO -- harden this and log any exceptions
            channels.Each(x => {
                _logger.InfoMessage(() => new EnvelopeSent(envelope, x));
                x.Send(envelope);
            });

            return envelope.CorrelationId;
        }
    }
}