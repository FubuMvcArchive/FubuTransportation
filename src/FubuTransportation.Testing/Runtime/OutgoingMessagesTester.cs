using FubuTransportation.Runtime;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class OutgoingMessagesTester
    {
        [Test]
        public void enqueue()
        {
            var messages = new OutgoingMessages();
            var m1 = new Message1();
            var m2 = new Message2();

            messages.Enqueue(m1);
            messages.Enqueue(m2);

            messages.ShouldHaveTheSameElementsAs(m1, m2);
        }

        [Test]
        public void nequeue_an_oject_array()
        {
            var messages = new OutgoingMessages();
            var m1 = new Message1();
            var m2 = new Message2();

            messages.Enqueue(new object[]{m1, m2});

            messages.ShouldHaveTheSameElementsAs(m1, m2);
        }
    }
}