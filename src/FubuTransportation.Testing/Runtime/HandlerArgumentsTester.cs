using FubuMVC.Core.Runtime;
using FubuTransportation.Runtime;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class HandlerArgumentsTester
    {
        [Test]
        public void enqueue()
        {
            var messages = new HandlerArguments(new Envelope{Message = new Message1()});
            var m1 = new Message1();
            var m2 = new Message2();

            messages.EnqueueCascading(m1);
            messages.EnqueueCascading(m2);

            messages.OutgoingMessages().ShouldHaveTheSameElementsAs(m1, m2);
        }

        [Test]
        public void nequeue_an_oject_array()
        {
            var messages = new HandlerArguments(new Envelope{Message = new Message1()});
            var m1 = new Message1();
            var m2 = new Message2();

            messages.EnqueueCascading(new object[]{m1, m2});

            messages.OutgoingMessages().ShouldHaveTheSameElementsAs(m1, m2);
        }
    }

    [TestFixture]
    public class when_building_a_new_handler_arguments_object
    {
        private Envelope theEnvelope;
        private HandlerArguments theArgs;

        [SetUp]
        public void SetUp()
        {
            theEnvelope = new Envelope{Message = new Message2()};

            theArgs = new HandlerArguments(theEnvelope);
        }

        [Test]
        public void should_set_an_IFubuRequest_with_the_message_set()
        {
            theArgs.Get<IFubuRequest>().Get<Message2>()
                   .ShouldBeTheSameAs(theEnvelope.Message);
        }

        [Test]
        public void registers_itself_as_the_outgoing_messages()
        {
            theArgs.Get<IInvocationContext>().ShouldBeTheSameAs(theArgs);
        }

        [Test]
        public void registers_The_envelope()
        {
            theArgs.Get<Envelope>().ShouldBeTheSameAs(theEnvelope);
        }
    }
}