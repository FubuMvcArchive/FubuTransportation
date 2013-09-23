using System;
using FubuTestingSupport;
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
                ClassUnderTest.Serialize(theEnvelope);
            }).Message.ShouldContain("random/nonexistent");
        }

        [Test]
        public void throws_on_serialize_with_no_message()
        {
            Exception<InvalidOperationException>.ShouldBeThrownBy(() => {
                ClassUnderTest.Serialize(new Envelope());
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

    }
}