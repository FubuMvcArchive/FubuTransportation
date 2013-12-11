using System;
using System.Linq;
using System.Runtime.InteropServices;
using FubuTestingSupport;
using FubuTransportation.Configuration;
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

        [Test]
        public void create_a_transport_node_from_a_channel_graph()
        {
            var graph = new ChannelGraph
            {
                Name = "Service1"
            };

            graph.AddReplyChannel("memory", "memory://replies".ToUri());
            graph.AddReplyChannel("foo", "foo://replies".ToUri());
            graph.AddReplyChannel("bar", "bar://replies".ToUri());

            var node = new TransportNode(graph);

            node.NodeName.ShouldEqual("Service1");

            node.Addresses.OrderBy(x => x.ToString()).ShouldHaveTheSameElementsAs("bar://replies".ToUri(), "foo://replies".ToUri(), "memory://replies".ToUri());
        }

        
    }
}