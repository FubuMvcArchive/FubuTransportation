using System.Diagnostics;
using FubuTestingSupport;
using FubuTransportation.Subscriptions;
using FubuTransportation.Testing.Events;
using NUnit.Framework;

namespace FubuTransportation.Testing.Subscriptions
{
    [TestFixture]
    public class Subscription_equality_tester
    {
        [Test]
        public void equals_if_all_are_equal()
        {
            var s1 = new Subscription(typeof(Message1))
            {
                NodeName = "Service1",
                Receiver = "foo://1".ToUri(),
                Source = "foo://2".ToUri()
            };

            var s2 = new Subscription(typeof(Message1))
            {
                NodeName = s1.NodeName,
                Receiver = s1.Receiver,
                Source = s1.Source
            };

            s1.ShouldEqual(s2);
            s2.ShouldEqual(s1);

            s2.NodeName = "different";
            s1.ShouldNotEqual(s2);

            s2.NodeName = s1.NodeName;
            s2.MessageType = typeof (Message2).AssemblyQualifiedName;
            s2.ShouldNotEqual(s1);

            s2.MessageType = s1.MessageType;
            s2.Receiver = "foo://3".ToUri();
            s2.ShouldNotEqual(s1);

            s2.Receiver = s1.Receiver;
            s2.Source = "foo://4".ToUri();
            s2.ShouldNotEqual(s1);
        }
    }
}