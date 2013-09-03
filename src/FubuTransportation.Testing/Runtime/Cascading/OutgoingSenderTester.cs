using FubuTestingSupport;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Cascading;
using NUnit.Framework;
using Rhino.Mocks;

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
    }

    

}