using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Scheduling;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Scheduling
{
    [TestFixture]
    public class ShutdownChannelNodeVisitorTester
    {
        [Test]
        public void shuts_down_the_channel()
        {
            var channel = new ChannelNode();
            channel.Scheduler = MockRepository.GenerateMock<IScheduler>();
            channel.Channel = MockRepository.GenerateMock<IChannel>();
            var visitor = new ShutdownChannelNodeVisitor();
            visitor.Visit(channel);
            channel.Channel.AssertWasCalled(x => x.Dispose());
            channel.Scheduler.AssertWasCalled(x => x.Dispose());
        }
    }
}