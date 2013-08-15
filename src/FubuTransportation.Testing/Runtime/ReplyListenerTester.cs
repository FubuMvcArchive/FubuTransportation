using System;
using System.Threading.Tasks;
using FubuTransportation.Logging;
using FubuTransportation.Runtime;
using NUnit.Framework;
using Rhino.Mocks;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class when_receiving_the_matching_reply
    {
        private IEventAggregator theEvents;
        public readonly string correlationId = Guid.NewGuid().ToString();
        private ReplyListener<Message1> theListener;
        private Message1 theMessage;

        [SetUp]
        public void SetUp()
        {
            theEvents = MockRepository.GenerateMock<IEventAggregator>();

            theListener = new ReplyListener<Message1>(theEvents, correlationId);

            theMessage = new Message1();

            var envelope = new Envelope
            {
                Message = theMessage
            };

            envelope.Headers[Envelope.ResponseIdKey] = correlationId;

            theListener.Handle(new EnvelopeReceived
            {
                Envelope = envelope
            });
        }

        [Test]
        public void should_set_the_completion_value()
        {
            theListener.Task.Result.ShouldBeTheSameAs(theMessage);
        }

        [Test]
        public void should_remove_itself_from_the_event_aggregator()
        {
            theEvents.AssertWasCalled(x => x.RemoveListener(theListener));
        }
    }

    [TestFixture]
    public class ReplyListenerMatchesTester
    {
        private IEventAggregator theEvents;
        public readonly string correlationId = Guid.NewGuid().ToString();
        private ReplyListener<Message1> theListener;
        private Message1 theMessage;

        [SetUp]
        public void SetUp()
        {
            theEvents = MockRepository.GenerateMock<IEventAggregator>();
            theListener = new ReplyListener<Message1>(theEvents, correlationId);
        }

        [Test]
        public void matches_if_type_is_right_and_correlation_id_matches()
        {
            theListener.Matches(new Envelope
            {
                ResponseId = correlationId,
                Message = new Message1()
            }).ShouldBeTrue();
        }

        [Test]
        public void does_not_match_if_correlation_id_is_wrong()
        {
            theListener.Matches(new Envelope
            {
                ResponseId = Guid.NewGuid().ToString(),
                Message = new Message1()
            }).ShouldBeFalse();
        }

        [Test]
        public void does_not_match_if_the_message_type_is_wrong()
        {
            theListener.Matches(new Envelope
            {
                ResponseId = correlationId,
                Message = new Message2()
            }).ShouldBeFalse();
        }
    }
}