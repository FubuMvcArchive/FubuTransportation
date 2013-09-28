using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Scheduling;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Scheduling
{
    [TestFixture]
    public class StartingChannelNodeVisitorTester
    {
        [Test]
        public void can_start_ChannelNode()
        {
            var scheduler = MockRepository.GenerateMock<IScheduler>();
            var receiver = MockRepository.GenerateMock<IReceiver>();
            var channel = MockRepository.GenerateMock<IChannel>();
            var visitor = new StartingChannelNodeVisitor(receiver);
            var channelNode = new ChannelNode();
            channelNode.Scheduler = scheduler;
            channelNode.Channel = channel;
            visitor.Visit(channelNode);
            scheduler.AssertWasCalled(x => x.Start(() => { }, false), x => x.IgnoreArguments());
        }
    }
}