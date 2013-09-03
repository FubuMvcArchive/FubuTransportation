using System;
using System.Threading.Tasks;
using FubuCore;
using FubuTransportation.Events;
using FubuTransportation.Logging;
using FubuTransportation.Runtime;
using NUnit.Framework;
using Rhino.Mocks;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class ReplayListener_expiration_logic_Tester
    {
        [Test]
        public void uses_the_expiration_time()
        {
            var listener = new ReplyListener<Events.Message1>(null, Guid.NewGuid().ToString(), 10.Minutes());

            listener.IsExpired.ShouldBeFalse();

            listener.ExpiresAt.ShouldNotBeNull();
            (listener.ExpiresAt > DateTime.UtcNow.AddMinutes(9)).ShouldBeTrue();
            (listener.ExpiresAt < DateTime.UtcNow.AddMinutes(11)).ShouldBeTrue();
        }
    }

    [TestFixture]
    public class when_receiving_the_matching_reply
    {
        private IEventAggregator theEvents;
        public readonly string correlationId = Guid.NewGuid().ToString();
        private ReplyListener<Events.Message1> theListener;
        private Events.Message1 theMessage;

        [SetUp]
        public void SetUp()
        {
            theEvents = MockRepository.GenerateMock<IEventAggregator>();

            theListener = new ReplyListener<Events.Message1>(theEvents, correlationId, 10.Minutes());

            theMessage = new Events.Message1();
            
            var envelope = new EnvelopeToken
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
        private ReplyListener<Events.Message1> theListener;
        private Events.Message1 theMessage;

        [SetUp]
        public void SetUp()
        {
            theEvents = MockRepository.GenerateMock<IEventAggregator>();
            theListener = new ReplyListener<Events.Message1>(theEvents, correlationId, 10.Minutes());
        }

        [Test]
        public void matches_if_type_is_right_and_correlation_id_matches()
        {
            theListener.Matches(new EnvelopeToken
            {
                ResponseId = correlationId,
                Message = new Events.Message1()
            }).ShouldBeTrue();
        }

        [Test]
        public void does_not_match_if_correlation_id_is_wrong()
        {
            theListener.Matches(new EnvelopeToken
            {
                ResponseId = Guid.NewGuid().ToString(),
                Message = new Events.Message1()
            }).ShouldBeFalse();
        }

        [Test]
        public void does_not_match_if_the_message_type_is_wrong()
        {
            theListener.Matches(new EnvelopeToken
            {
                ResponseId = correlationId,
                Message = new Events.Message2()
            }).ShouldBeFalse();
        }
    }
}