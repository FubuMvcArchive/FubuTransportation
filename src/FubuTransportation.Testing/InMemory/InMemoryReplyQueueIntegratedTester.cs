using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using NUnit.Framework;
using FubuMVC.StructureMap;
using StructureMap;
using System.Linq;
using FubuTestingSupport;

namespace FubuTransportation.Testing.InMemory
{
    [TestFixture]
    public class InMemoryReplyQueueIntegratedTester
    {
        private ChannelGraph graph;
        private ChannelNode theReplyNode;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            graph = FubuTransport.For(x => { }).StructureMap(new Container())
                                 .Bootstrap().Factory.Get<ChannelGraph>();

            theReplyNode = graph.FirstOrDefault(x => x.ForReplies && x.Protocol() == InMemoryChannel.Protocol);
        }

        [Test]
        public void is_a_reply_node()
        {
            theReplyNode.ShouldNotBeNull();
        }

        [Test]
        public void should_be_incoming()
        {
            theReplyNode.Incoming.ShouldBeTrue();
        }

        [Test]
        public void should_be_reply_flag()
        {
            theReplyNode.ForReplies.ShouldBeTrue();
        }

        [Test]
        public void key_is_derivative_from_the_transport()
        {
            theReplyNode.Key.ShouldEqual("memory:replies");
        }

        [Test]
        public void should_have_a_channel()
        {
            theReplyNode.Channel.ShouldBeOfType<InMemoryChannel>();
        }
    }
}