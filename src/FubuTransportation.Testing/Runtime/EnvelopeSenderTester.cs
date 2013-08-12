using System;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Testing.TestSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class when_sending_a_message : InteractionContext<EnvelopeSender>
    {
        private StubChannelNode node1;
        private StubChannelNode node2;
        private StubChannelNode node3;
        private Message theMessage;
        private byte[] theData;
        private Envelope theEnvelope;
        private string correlationId;

        protected override void beforeEach()
        {
            node1 = new StubChannelNode();
            node2 = new StubChannelNode();
            node3 = new StubChannelNode();

            theMessage = new Message();
            theEnvelope = new Envelope {Message = theMessage};
            

            MockFor<IChannelRouter>().Stub(x => x.FindChannels(theMessage))
                                     .Return(new ChannelNode[] { node1, node2, node3 });

            correlationId = ClassUnderTest.Send(theEnvelope);

        }

        [Test]
        public void should_serialize_the_envelope()
        {
            MockFor<IEnvelopeSerializer>().AssertWasCalled(x => x.Serialize(theEnvelope));
        }

        [Test]
        public void all_nodes_receive_teh_message()
        {
            node1.LastEnvelope.ShouldBeTheSameAs(theEnvelope);
            node2.LastEnvelope.ShouldBeTheSameAs(theEnvelope);
            node3.LastEnvelope.ShouldBeTheSameAs(theEnvelope);
        }

        [Test]
        public void adds_a_correlation_id_to_the_envelope()
        {
            Guid.Parse(theEnvelope.CorrelationId)
                .ShouldNotEqual(Guid.Empty);

            theEnvelope.CorrelationId.ShouldEqual(correlationId);
        }
    }

    public class StubChannelNode : ChannelNode
    {
        public Envelope LastEnvelope;

        public override void Send(Envelope envelope)
        {
            LastEnvelope = envelope;
        }
    }
}