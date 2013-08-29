using System;
using FubuCore.Logging;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Logging;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Serializers;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using Rhino.Mocks;
using FubuCore;

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

            MockFor<ISubscriptions>().Stub(x => x.FindChannels(theEnvelope))
                                     .Return(new ChannelNode[] { node1, node2, node3 });

            correlationId = ClassUnderTest.Send(theEnvelope);

        }

        [Test]
        public void should_audit_each_node_sender_for_the_envelope()
        {
            theLogger.InfoMessages.ShouldContain(new EnvelopeSent(theEnvelope.ToToken(), node1));
            theLogger.InfoMessages.ShouldContain(new EnvelopeSent(theEnvelope.ToToken(), node2));
            theLogger.InfoMessages.ShouldContain(new EnvelopeSent(theEnvelope.ToToken(), node3));
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

        public StubChannelNode(string protocol = null)
        {
            Key = Guid.NewGuid().ToString();
            Uri = ("{0}://{1}".ToFormat(protocol ?? "fake", Key)).ToUri();
        }

        public override void Send(Envelope envelope, ChannelNode replyNode = null)
        {
            if (replyNode != null)
            {
                envelope.ReplyUri = replyNode.Uri;
            }

            LastEnvelope = envelope;

        }
    }
}