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
        private IScheduler scheduler;
        private IChannel channel;
        private StartingChannelNodeVisitor visitor;
        private ChannelNode channelNode;

        [SetUp]
        public void Setup()
        {
            scheduler = MockRepository.GenerateMock<IScheduler>();
            var receiver = MockRepository.GenerateMock<IReceiver>();
            channel = MockRepository.GenerateMock<IChannel>();
            visitor = new StartingChannelNodeVisitor(receiver);
            channelNode = new ChannelNode {Scheduler = scheduler, Channel = channel};
        }

        [Test]
        public void can_start_ChannelNode()
        {
            visitor.Visit(channelNode);
            scheduler.AssertWasCalled(x => x.Start(() => { }), x => x.IgnoreArguments());
        }

        [Test]
        public void will_loop_if_receiving_state_can()
        {
            channel.Expect(x => x.Receive(null)).IgnoreArguments().Return(ReceivingState.StopReceiving);
            visitor.StartReceive(ReceivingState.CanContinueReceiving, channel);
            channel.VerifyAllExpectations();
        }

        [Test]
        public void will_not_loop_if_receiving_state_cant()
        {
            visitor.StartReceive(ReceivingState.StopReceiving, channel);
            channel.AssertWasNotCalled(x => x.Receive(null), x => x.IgnoreArguments());
        }
    }
}