using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime.Serializers
{
    public interface IEnvelopeSerializer
    {
        object Deserialize(Envelope envelope);
        void Serialize(Envelope envelope);
    }

    public class EnvelopeSerializer : IEnvelopeSerializer
    {
        private readonly ChannelGraph _graph;
        private readonly IEnumerable<IMessageSerializer> _serializers;

        public EnvelopeSerializer(ChannelGraph graph, IEnumerable<IMessageSerializer> serializers)
        {
            _graph = graph;
            _serializers = serializers;
        }

        public object Deserialize(Envelope envelope)
        {
            if (envelope.Data == null) throw new EnvelopeDeserializationException("No data on this envelope to deserialize");


            var serializer = selectSerializer(envelope);
            
            using (var stream = new MemoryStream(envelope.Data))
            {
                return serializer.Deserialize(stream);
            }
        }

        private IMessageSerializer selectSerializer(Envelope envelope)
        {
            var serializer = _serializers.FirstOrDefault(x => x.ContentType.EqualsIgnoreCase(envelope.ContentType));
        
            if (serializer == null)
            {
                throw new EnvelopeDeserializationException("Unknown content-type '{0}'".ToFormat(envelope.ContentType));
            }

            return serializer;
        }

        public void Serialize(Envelope envelope)
        {
            if (envelope.Message == null) throw new InvalidOperationException("No message on this envelope to serialize");

            if (envelope.ContentType.IsEmpty())
            {
                envelope.ContentType = _graph.DefaultContentType;
            }

            var serializer = selectSerializer(envelope);
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(envelope.Message, stream);
                stream.Position = 0;

                envelope.Data = stream.ReadAllBytes();
            }
        }
    }
}