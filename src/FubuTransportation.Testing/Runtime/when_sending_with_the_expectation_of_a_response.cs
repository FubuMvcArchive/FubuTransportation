using FubuCore.Logging;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class when_sending_with_the_expectation_of_a_response : InteractionContext<EnvelopeSender>
    {
        private StubChannelNode destinationNode;
        private StubChannelNode replyNode;
        private Message theMessage;
        private Envelope theEnvelope;
        private RecordingLogger theLogger;

        protected override void beforeEach()
        {
            destinationNode = new StubChannelNode("fake");
            replyNode = new StubChannelNode("fake"){ForReplies = true};

            theMessage = new Message();
            theEnvelope = new Envelope { Message = theMessage, ReplyRequested = true};

            theLogger = new RecordingLogger();
            Services.Inject<ILogger>(theLogger);

            MockFor<ISubscriptions>().Stub(x => x.FindChannels(theEnvelope))
                                     .Return(new ChannelNode[] { destinationNode });

            MockFor<ISubscriptions>().Stub(x => x.ReplyNodeFor(destinationNode)).Return(replyNode);

            ClassUnderTest.Send(theEnvelope);
        }

        [Test]
        public void should_have_associated_the_reply_channel_with_the_envelope()
        {
            theEnvelope.ReplyUri.ShouldEqual(replyNode.Uri);
        }
    }
}