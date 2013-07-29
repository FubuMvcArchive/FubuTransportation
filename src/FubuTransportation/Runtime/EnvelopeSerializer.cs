using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;

namespace FubuTransportation.Runtime
{
    public interface IEnvelopeSerializer
    {
        void Deserialize(Envelope envelope);
        void Serialize(Envelope envelope);

        // TODO -- maybe?
        // string ContentTypeFor<T>() ??
    }

    public class EnvelopeSerializer : IEnvelopeSerializer
    {
        private readonly IEnumerable<IMessageSerializer> _serializers;

        public EnvelopeSerializer(IEnumerable<IMessageSerializer> serializers)
        {
            _serializers = serializers;
        }

        public void Deserialize(Envelope envelope)
        {
            var serializer = _serializers.FirstOrDefault(x => x.ContentType.EqualsIgnoreCase(envelope.ContentType));
            // TODO -- what to do w/ unknown content-type?

            using (var stream = new MemoryStream(envelope.Data))
            {
                envelope.Message = serializer.Deserialize(stream);
            }

        }

        public void Serialize(Envelope envelope)
        {
            throw new NotImplementedException();
        }
    }
}