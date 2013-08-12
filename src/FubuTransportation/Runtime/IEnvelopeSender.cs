using System;
using System.Collections.Generic;
using System.Linq;

namespace FubuTransportation.Runtime
{
    public interface IEnvelopeSender
    {
        string Send(Envelope envelope);
    }

    public class EnvelopeSender : IEnvelopeSender
    {
        private readonly IChannelRouter _router;
        private readonly IEnvelopeSerializer _serializer;

        public EnvelopeSender(IChannelRouter router, IEnvelopeSerializer serializer)
        {
            _router = router;
            _serializer = serializer;
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

            channels.Each(x => x.Send(envelope));

            return envelope.CorrelationId;
        }
    }
}