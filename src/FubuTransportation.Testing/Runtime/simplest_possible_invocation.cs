using FubuTransportation.Testing.TestSupport;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class simplest_possible_invocation : InvocationContext
    {
        [Test]
        public void single_message_for_single_handler_one()
        {
            handler<OneHandler, TwoHandler, ThreeHandler, FourHandler>();

            var theMessage = new OneMessage();
            sendMessage(theMessage);

            TestMessageRecorder.AllProcessed.Single()
                               .ShouldMatch<OneHandler>(theMessage);
        }

        [Test]
        public void single_message_for_single_handler_two()
        {
            handler<OneHandler, TwoHandler, ThreeHandler, FourHandler>();

            var theMessage = new TwoMessage();
            sendMessage(theMessage);

            TestMessageRecorder.AllProcessed.Single()
                               .ShouldMatch<TwoHandler>(theMessage);


        }

        [Test]
        public void successful_processing_calls_back_on_the_envelope()
        {
            handler<OneHandler, TwoHandler, ThreeHandler, FourHandler>();

            var theMessage = new TwoMessage();


            sendMessage(theMessage).Callback.AssertWasCalled(x => x.MarkSuccessful());

        }

        [Test]
        public void single_message_for_single_handler_three()
        {
            handler<OneHandler, TwoHandler, ThreeHandler, FourHandler>();

            var theMessage = new ThreeMessage();
            sendMessage(theMessage);

            TestMessageRecorder.AllProcessed.Single()
                               .ShouldMatch<ThreeHandler>(theMessage);
        }

        [Test]
        public void single_message_for_single_handler_four()
        {
            handler<OneHandler, TwoHandler, ThreeHandler, FourHandler>();

            var theMessage = new FourMessage();
            sendMessage(theMessage);

            TestMessageRecorder.AllProcessed.Single()
                               .ShouldMatch<FourHandler>(theMessage);
        }
    }
}