using FubuTransportation.InMemory;
using FubuTransportation.Runtime;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;

namespace FubuTransportation.Testing.InMemory
{
    [TestFixture]
    public class InMemoryCallbackTester
    {
        [Test]
        public void moves_to_delayed_queue()
        {
            InMemoryQueueManager.ClearAll();

            var envelope = new EnvelopeToken();
            var callback = new InMemoryCallback(null, envelope);
            callback.MoveToDelayed();

            InMemoryQueueManager.DelayedEnvelopes()
                                .Single().CorrelationId.ShouldEqual(envelope.CorrelationId);
        }
    }
}