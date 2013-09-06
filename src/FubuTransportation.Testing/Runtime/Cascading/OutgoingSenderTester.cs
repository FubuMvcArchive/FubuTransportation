using System;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Cascading;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;

namespace FubuTransportation.Testing.Runtime.Cascading
{
    [TestFixture]
    public class OutgoingSenderTester : InteractionContext<OutgoingSender>
    {
        [Test]
        public void use_envelope_from_the_original_if_not_ISendMyself()
        {
            var original = MockRepository.GenerateMock<Envelope>();
            var message = new Events.Message1();

            var resulting = new Envelope();

            original.Expect(x => x.ForResponse(message)).Return(resulting);

            ClassUnderTest.SendOutgoingMessage(original, message);

            MockFor<IEnvelopeSender>().AssertWasCalled(x => x.Send(resulting));
        }

        [Test]
        public void use_envelope_from_ISendMySelf()
        {
            var message = MockRepository.GenerateMock<ISendMyself>();
            var original = new Envelope();
            var resulting = new Envelope();

            message.Stub(x => x.CreateEnvelope(original)).Return(resulting);

            ClassUnderTest.SendOutgoingMessage(original, message);

            MockFor<IEnvelopeSender>().AssertWasCalled(x => x.Send(resulting));
        }

        [Test]
        public void if_original_envelope_is_ack_requested_send_ack_back()
        {
            var original = new Envelope
            {
                ReplyUri = "foo://bar".ToUri(),
                AckRequested = true,
                CorrelationId = Guid.NewGuid().ToString()
            };

            ClassUnderTest.SendOutgoingMessages(original, new object[0]);

            var envelope = MockFor<IEnvelopeSender>().GetArgumentsForCallsMadeOn(x => x.Send(null))[0][0]
                .As<Envelope>();
            envelope.ShouldNotBeNull();

            envelope.ResponseId.ShouldEqual(original.CorrelationId);
            envelope.Destination.ShouldEqual(original.ReplyUri);
            envelope.Message.ShouldEqual(new Acknowledgement {CorrelationId = original.CorrelationId});
        }

        [Test]
        public void do_not_send_ack_if_no_ack_is_requested()
        {
            var original = new Envelope
            {
                ReplyUri = "foo://bar".ToUri(),
                AckRequested = false,
                CorrelationId = Guid.NewGuid().ToString()
            };

            ClassUnderTest.SendOutgoingMessages(original, new object[0]);

            MockFor<IEnvelopeSender>().AssertWasNotCalled(x => x.Send(null), x => x.IgnoreArguments());
        }
    }

    

}