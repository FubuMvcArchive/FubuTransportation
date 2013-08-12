using System;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Testing.TestSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class when_sending_a_message : InteractionContext<ServiceBus>
    {
        private StubChannelNode node1;
        private StubChannelNode node2;
        private StubChannelNode node3;
        private Message theMessage;
        private byte[] theData;

        protected override void beforeEach()
        {
            node1 = new StubChannelNode();
            node2 = new StubChannelNode();
            node3 = new StubChannelNode();

            theMessage = new Message();

            MockFor<IChannelRouter>().Stub(x => x.FindChannels(theMessage))
                                     .Return(new ChannelNode[] {node1, node2, node3});

            ClassUnderTest.Send(theMessage);

        }

        [Test]
        public void should_serialize_the_envelope()
        {
            MockFor<IEnvelopeSerializer>().AssertWasCalled(x => x.Serialize(node1.LastEnvelope));
        }

        [Test]
        public void the_message_should_be_in_the_envelope()
        {
            node1.LastEnvelope.Message.ShouldBeTheSameAs(theMessage);
        }

        [Test]
        public void all_nodes_receive_teh_message()
        {
            node1.LastEnvelope.Message.ShouldBeTheSameAs(theMessage);
            node2.LastEnvelope.Message.ShouldBeTheSameAs(theMessage);
            node3.LastEnvelope.Message.ShouldBeTheSameAs(theMessage);
        }

        [Test]
        public void adds_a_correlation_id_to_the_envelope()
        {
            Guid.Parse(node1.LastEnvelope.CorrelationId)
                .ShouldNotEqual(Guid.Empty);
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