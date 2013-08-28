using FubuCore.Logging;
using FubuMVC.Core.Runtime.Logging;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Runtime.Invocation
{
    [TestFixture]
    public class ChainExecutionTester
    {
        [Test]
        public void delegate_to_chain_invoker()
        {
            var invoker = MockRepository.GenerateMock<IMessageInvoker>();
            var chain = new HandlerChain();
            var envelope = ObjectMother.Envelope();

            var continuation = new ChainExecution(invoker, chain);
            continuation.Execute(envelope, new RecordingLogger());

            invoker.AssertWasCalled(x => x.ExecuteChain(envelope, chain));
        }
    }
}