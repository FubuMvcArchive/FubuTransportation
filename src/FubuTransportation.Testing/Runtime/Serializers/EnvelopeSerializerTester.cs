using System;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Serializers;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Runtime.Serializers
{
    [TestFixture]
    public class EnvelopeSerializerTester : InteractionContext<EnvelopeSerializer>
    {
        private IMessageSerializer[] serializers;
        private Envelope theEnvelope;

        protected override void beforeEach()
        {
            serializers = Services.CreateMockArrayFor<IMessageSerializer>(5);
            for (int i = 0; i < serializers.Length; i++)
            {
                serializers[i].Stub(x => x.ContentType).Return("text/" + i);
            }

            theEnvelope = new Envelope()
            {
                Data = new byte[0]
            };
        }

        [Test]
        public void chooses_by_mimetype()
        {
            theEnvelope.ContentType = serializers[3].ContentType;
            var o = new object();
            serializers[3].Stub(x => x.Deserialize(null)).IgnoreArguments().Return(o);

            ClassUnderTest.Deserialize(theEnvelope).ShouldBeTheSameAs(o);
        }

        [Test]
        public void throws_on_unknown_content_type()
        {
            theEnvelope.ContentType = "random/nonexistent";
            theEnvelope.Message = new object();

            Exception<EnvelopeDeserializationException>.ShouldBeThrownBy(() => {
                ClassUnderTest.Serialize(theEnvelope, new ChannelNode());
            }).Message.ShouldContain("random/nonexistent");
        }
        
        [Test]
        public void throws_on_serialize_with_no_message()
        {
            Exception<InvalidOperationException>.ShouldBeThrownBy(() => {
                ClassUnderTest.Serialize(new Envelope(), new ChannelNode());
            }).Message.ShouldEqual("No message on this envelope to serialize");
        }

        [Test]
        public void throws_on_deserialize_with_no_data()
        {
            Exception<EnvelopeDeserializationException>.ShouldBeThrownBy(() =>
            {
                ClassUnderTest.Deserialize(new Envelope());
            }).Message.ShouldEqual("No data on this envelope to deserialize");
        }


        [Test]
        public void select_serializer_uses_the_envelope_override_if_it_exists()
        {
            var node = new ChannelNode
            {
                DefaultContentType = serializers[1].ContentType
            };
            MockFor<ChannelGraph>().DefaultContentType = serializers[4].ContentType;

            theEnvelope.ContentType = serializers[3].ContentType;

            ClassUnderTest.SelectSerializer(theEnvelope, node)
                .ShouldBeTheSameAs(serializers[3]);
        }

        [Test]
        public void select_the_graph_default_in_the_absence_of_everything_else()
        {
            MockFor<ChannelGraph>().DefaultContentType = serializers[4].ContentType;
            ClassUnderTest.SelectSerializer(theEnvelope, new ChannelNode())
                .ShouldBeTheSameAs(serializers[4]);

        }

        [Test]
        public void use_channel_node_default_content_type_if_it_exists_and_not_set_on_the_envelope()
        {
            MockFor<ChannelGraph>().DefaultContentType = serializers[4].ContentType;
            var node = new ChannelNode
            {
                DefaultContentType = serializers[1].ContentType
            };

            ClassUnderTest.SelectSerializer(theEnvelope, node)
                .ShouldBeTheSameAs(serializers[1]);
        }

        [Test]
        public void use_a_serializer_on_the_channel_node_as_the_default_if_content_type_is_not_explicitly_set()
        {
            MockFor<ChannelGraph>().DefaultContentType = serializers[4].ContentType;
            var node = new ChannelNode
            {
                DefaultSerializer = MockRepository.GenerateMock<IMessageSerializer>()
            };

            ClassUnderTest.SelectSerializer(theEnvelope, node)
                .ShouldBeTheSameAs(node.DefaultSerializer);
        }

        [Test]
        public void ask_for_a_content_type_that_does_not_exist()
        {
            theEnvelope.ContentType = "weird";
            Exception<EnvelopeDeserializationException>.ShouldBeThrownBy(() => {
                ClassUnderTest.SelectSerializer(theEnvelope, new ChannelNode());
            });
        }


    }
}