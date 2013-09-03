using FubuMVC.Core.Runtime;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Runtime.Invocation
{
    [TestFixture]
    public class InvocationContextTester
    {
        [Test]
        public void enqueue()
        {
            var messages = new FubuTransportation.Runtime.Invocation.InvocationContext(new Envelope{Message = new Events.Message1()});
            var m1 = new Events.Message1();
            var m2 = new Events.Message2();

            messages.EnqueueCascading(m1);
            messages.EnqueueCascading(m2);

            messages.OutgoingMessages().ShouldHaveTheSameElementsAs(m1, m2);
        }

        [Test]
        public void enqueue_an_oject_array()
        {
            var messages = new FubuTransportation.Runtime.Invocation.InvocationContext(new Envelope{Message = new Events.Message1()});
            var m1 = new Events.Message1();
            var m2 = new Events.Message2();

            messages.EnqueueCascading(new object[]{m1, m2});

            messages.OutgoingMessages().ShouldHaveTheSameElementsAs(m1, m2);
        }
    }

    [TestFixture]
    public class when_building_a_new_handler_arguments_object
    {
        private Envelope theEnvelope;
        private FubuTransportation.Runtime.Invocation.InvocationContext theArgs;

        [SetUp]
        public void SetUp()
        {
            theEnvelope = new Envelope{Message = new Events.Message2()};

            theArgs = new FubuTransportation.Runtime.Invocation.InvocationContext(theEnvelope);
        }

        [Test]
        public void should_set_an_IFubuRequest_with_the_message_set()
        {
            theArgs.Get<IFubuRequest>().Get<Events.Message2>()
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