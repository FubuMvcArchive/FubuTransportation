using FubuTransportation.Configuration;
using FubuTransportation.Runtime;

namespace FubuTransportation.Scheduling
{
    public class StartingChannelNodeVisitor : IChannelNodeVisitor
    {
        private readonly IReceiver _receiver;

        public StartingChannelNodeVisitor(IReceiver receiver)
        {
            _receiver = receiver;
        }

        public void Visit(ChannelNode node)
        {
            node.Scheduler.Start(() => node.Channel.Receive(_receiver));
        }
    }
}