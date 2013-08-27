using System.Linq;
using FubuTestingSupport;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;

namespace FubuTransportation.Testing.Runtime.Invocation
{
    [TestFixture]
    public class handling_a_batch_of_messages : InvocationContext
    {
        [Test]
        public void generic_handler_is_applied_at_end()
        {
            handler<OneHandler, TwoHandler, ThreeHandler, GenericHandler>();

            var message1 = new OneMessage();
            var message2 = new TwoMessage();
            var message3 = new ThreeMessage();

            sendMessage(message1, message2, message3);

            TestMessageRecorder.AllProcessed.Count().ShouldEqual(6);
            TestMessageRecorder.AllProcessed[0].ShouldMatch<OneHandler>(message1);
            TestMessageRecorder.AllProcessed[1].ShouldMatch<GenericHandler>(message1);
            TestMessageRecorder.AllProcessed[2].ShouldMatch<TwoHandler>(message2);
            TestMessageRecorder.AllProcessed[3].ShouldMatch<GenericHandler>(message2);
            TestMessageRecorder.AllProcessed[4].ShouldMatch<ThreeHandler>(message3);
            TestMessageRecorder.AllProcessed[5].ShouldMatch<GenericHandler>(message3);
        }
    }
}