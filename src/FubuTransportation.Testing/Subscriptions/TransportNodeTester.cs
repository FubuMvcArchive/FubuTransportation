using System;
using FubuTestingSupport;
using FubuTransportation.Subscriptions;
using NUnit.Framework;

namespace FubuTransportation.Testing.Subscriptions
{
    [TestFixture]
    public class TransportNodeTester
    {
        [Test]
        public void equals_positive()
        {
            var node1 = new TransportNode
            {
                Addresses = new Uri[] {"foo://1".ToUri(), "bar://1".ToUri()}
            };

            var node2 = new TransportNode
            {
                Addresses = new Uri[] { "bar://1".ToUri(), "foo://1".ToUri() }
            };

            node1.ShouldEqual(node2);
            node2.ShouldEqual(node1);
        }

        [Test]
        public void equals_negative()
        {
            var node1 = new TransportNode
            {
                Addresses = new Uri[] { "foo://1".ToUri(), "bar://1".ToUri() }
            };

            var node2 = new TransportNode
            {
                Addresses = new Uri[] { "bar://3".ToUri(), "foo://1".ToUri() }
            };

            node1.ShouldNotEqual(node2);
            node2.ShouldNotEqual(node1);
        }
    }
}