using System;
using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using FubuCore.Logging;
using FubuTransportation.Configuration;
using FubuTransportation.Scheduling;

namespace FubuTransportation.Runtime
{
    public class ChannelShutdownDeactivator : IDeactivator
    {
        private readonly ChannelGraph _graph;
        private readonly ILogger _logger;

        public ChannelShutdownDeactivator(ChannelGraph graph, ILogger logger)
        {
            _graph = graph;
            _logger = logger;
        }

        public void Deactivate(IPackageLog log)
        {
            _graph.Each(channel =>
            {
                try
                {
                    var shudownVisitor = new ShutdownChannelNodeVisitor();
                    shudownVisitor.Visit(channel);
                }
                catch (Exception ex)
                {
                    _logger.Error("Error while trying to shutdown the channel for " + channel.Uri, ex);
                }
            });
        }
    }
}