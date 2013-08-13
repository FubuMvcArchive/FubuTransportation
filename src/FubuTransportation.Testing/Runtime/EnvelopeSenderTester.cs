using System;
using FubuCore.Logging;
using FubuMVC.Core.Runtime.Logging;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Logging;
using FubuTransportation.Runtime;
using FubuTransportation.Testing.ScenarioSupport;
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
        private RecordingLogger theLogger;

        protected override void beforeEach()
        {
            node1 = new StubChannelNode();
            node2 = new StubChannelNode();
            node3 = new StubChannelNode();

            theMessage = new Message();
            theEnvelope = new Envelope {Message = theMessage};

            theLogger = new RecordingLogger();
            Services.Inject<ILogger>(theLogger);

            MockFor<IChannelRouter>().Stub(x => x.FindChannels(theEnvelope))
                                     .Return(new ChannelNode[] { node1, node2, node3 });

            correlationId = ClassUnderTest.Send(theEnvelope);

        }

        [Test]
        public void should_audit_each_node_sender_for_the_envelope()
        {
            theLogger.InfoMessages.ShouldContain(new EnvelopeSent(theEnvelope, node1));
            theLogger.InfoMessages.ShouldContain(new EnvelopeSent(theEnvelope, node2));
            theLogger.InfoMessages.ShouldContain(new EnvelopeSent(theEnvelope, node3));
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

        public StubChannelNode()
        {
            Key = Guid.NewGuid().ToString();
            Uri = ("fake://" + Key).ToUri();
        }

        public override void Send(Envelope envelope)
        {
            LastEnvelope = envelope;
        }
    }
}