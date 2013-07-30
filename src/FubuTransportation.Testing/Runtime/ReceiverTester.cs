using System;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class when_receiving_a_message : InteractionContext<Receiver>
    {
        Envelope envelope = new Envelope(null);
        Uri address = new Uri("foo://bar");
        private IChannel theChannel;

        protected override void beforeEach()
        {
            theChannel = MockFor<IChannel>();
            theChannel.Stub(x => x.Address).Return(address);
            ClassUnderTest.Receive(theChannel, envelope);
        }

        [Test]
        public void should_copy_the_channel_address_to_the_envelope()
        {
            envelope.Source.ShouldEqual(address);
        }

        [Test]
        public void should_call_through_to_the_invoker()
        {
            MockFor<IMessageInvoker>().AssertWasCalled(x => x.Invoke(envelope));
        }
    }
}