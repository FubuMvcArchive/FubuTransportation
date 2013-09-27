using System;
using FubuTransportation.Configuration;

namespace FubuTransportation.Scheduling
{
    public class ShutdownChannelNodeVisitor : IChannelNodeVisitor
    {
        public void Visit(ChannelNode node)
        {
            node.Scheduler.Dispose();
            var disposable = node.Channel as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}