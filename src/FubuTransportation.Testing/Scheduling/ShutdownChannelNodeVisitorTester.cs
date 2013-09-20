using FubuTransportation.Configuration;
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
            var visitor = new ShutdownChannelNodeVisitor();
            visitor.Visit(channel);
            channel.Scheduler.AssertWasCalled(x => x.Dispose());
        }
    }
}