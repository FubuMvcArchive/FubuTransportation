using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Logging;
using FubuTransportation.Logging;
using FubuTransportation.Runtime.Serializers;

namespace FubuTransportation.Runtime
{
    public class EnvelopeSender : IEnvelopeSender
    {
        private readonly ISubscriptions _router;
        private readonly IEnvelopeSerializer _serializer;
        private readonly ILogger _logger;
        private readonly IEnumerable<IEnvelopeModifier> _modifiers;

        public EnvelopeSender(ISubscriptions router, IEnvelopeSerializer serializer, ILogger logger, IEnumerable<IEnvelopeModifier> modifiers)
        {
            _router = router;
            _serializer = serializer;
            _logger = logger;
            _modifiers = modifiers;
        }

        // virtual for testing
        public string Send(Envelope envelope)
        {
            envelope.Headers[Envelope.MessageTypeKey] = envelope.Message.GetType().FullName;

            _modifiers.Each(x => x.Modify(envelope));
            _serializer.Serialize(envelope);
            

            var channels = _router.FindChannels(envelope).ToArray();

            // TODO -- needs more work and thought here about what to do.
            if (!channels.Any())
            {
                throw new Exception("No channels match this message");
            }

            // TODO -- harden this and log any exceptions
            channels.Each(x => {
                _logger.InfoMessage(() => new EnvelopeSent(envelope.ToToken(), x));

                x.Send(envelope, _router.ReplyNodeFor(x));
            });

            return envelope.CorrelationId;
        }


    }
}