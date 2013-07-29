using FubuTestingSupport;
using FubuTransportation.Runtime;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class TransportActivatorTester : InteractionContext<TransportActivator>
    {
        [Test]
        public void transports_are_started()
        {
            // TODO -- will be obsolete
            var transport = MockFor<ITransport>();
            transport.Expect(x => x.StartReceiving(Arg<ChannelOptions>.Is.Anything, Arg<IReceiver>.Is.Anything));
            ClassUnderTest.Activate(null, null);
            transport.VerifyAllExpectations();
        }
    }
}